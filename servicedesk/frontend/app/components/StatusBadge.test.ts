import { describe, expect, it } from "vitest";
import { mount } from "@vue/test-utils";
import StatusBadge from "./StatusBadge.vue";

describe("StatusBadge", () => {
  it("shows text as well as colour for ticket state", () => {
    const badge = mount(StatusBadge, {
      props: { label: "In progress", tone: "amber" },
    });
    expect(badge.text()).toBe("In progress");
    expect(badge.classes()).toContain("badge-amber");
  });
});
