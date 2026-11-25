import { expect, test, type Page } from "@playwright/test";

async function signIn(page: Page, email: string) {
  await page.goto("/login");
  await page.getByLabel("Email").fill(email);
  await page.getByLabel("Password").fill("Training!2026");
  await page.getByRole("button", { name: "Sign in" }).click();
  await expect(page.getByRole("button", { name: "Sign out" })).toBeVisible();
}

test("requester creates a ticket; coordinator assigns it; technician progresses it; another requester is denied", async ({
  browser,
}) => {
  const requester = await browser.newContext();
  const requesterPage = await requester.newPage();
  await signIn(requesterPage, "requester1@example.test");
  await requesterPage
    .getByRole("link", { name: "Tickets", exact: true })
    .click();
  await requesterPage
    .getByRole("link", { name: "+ New request", exact: true })
    .click();
  const title = `E2E laptop issue ${Date.now()}`;
  await requesterPage.getByLabel("Title *").fill(title);
  await requesterPage.getByLabel("Category *").selectOption({ index: 1 });
  await requesterPage
    .getByLabel("Description *")
    .fill("Battery fails after ten minutes; reproduced twice.");
  await requesterPage.getByRole("button", { name: "Create request" }).click();
  await expect(
    requesterPage.getByRole("heading", { name: title }),
  ).toBeVisible();
  const ticketPath = new URL(requesterPage.url()).pathname;
  await requesterPage
    .getByLabel("Add a comment")
    .fill("Reproduced again after a restart.");
  await requesterPage.getByRole("button", { name: "Post comment" }).click();
  await expect(
    requesterPage.getByText("Reproduced again after a restart."),
  ).toBeVisible();

  const other = await browser.newContext();
  const otherPage = await other.newPage();
  await signIn(otherPage, "requester2@example.test");
  await otherPage.goto(ticketPath);
  await expect(otherPage.getByRole("alert")).toContainText("access denied");

  const coordinator = await browser.newContext();
  const coordinatorPage = await coordinator.newPage();
  await signIn(coordinatorPage, "coordinator@example.test");
  await coordinatorPage.screenshot({
    path: "test-results/coordinator-desktop.png",
    fullPage: true,
  });
  await coordinatorPage.setViewportSize({ width: 390, height: 844 });
  await coordinatorPage.screenshot({
    path: "test-results/coordinator-mobile.png",
    fullPage: true,
  });
  await coordinatorPage.setViewportSize({ width: 1280, height: 720 });
  await coordinatorPage.goto("/tickets");
  await coordinatorPage.getByLabel("Search tickets").fill(title);
  await coordinatorPage.getByRole("button", { name: "Search" }).click();
  await expect(
    coordinatorPage.getByRole("heading", { name: "1 tickets" }),
  ).toBeVisible();
  await coordinatorPage.goto(ticketPath);
  await coordinatorPage
    .getByLabel("Assign technician")
    .selectOption({ label: "Jordan Lee" });
  await coordinatorPage
    .getByRole("button", { name: "Assign", exact: true })
    .click();
  await expect(
    coordinatorPage.getByText("Assigned to Jordan Lee"),
  ).toBeVisible();
  await coordinatorPage
    .getByLabel("Priority", { exact: true })
    .selectOption("Critical");
  await coordinatorPage
    .getByRole("button", { name: "Update priority" })
    .click();
  await expect(
    coordinatorPage.getByText("Priority changed to Critical"),
  ).toBeVisible();

  const technician = await browser.newContext();
  const technicianPage = await technician.newPage();
  await signIn(technicianPage, "technician1@example.test");
  await technicianPage.goto(ticketPath);
  await expect(
    technicianPage.getByRole("heading", { name: title }),
  ).toBeVisible();
  await expect(
    technicianPage.getByRole("button", { name: "Assign", exact: true }),
  ).toHaveCount(0);
  await technicianPage.getByRole("button", { name: "In progress" }).click();
  await expect(
    technicianPage.getByRole("button", { name: "Resolved" }),
  ).toBeVisible();
  await technicianPage.getByRole("button", { name: "Resolved" }).click();
  await expect(
    technicianPage.getByText("Status changed to Resolved"),
  ).toBeVisible();

  await requester.close();
  await other.close();
  await coordinator.close();
  await technician.close();
});
