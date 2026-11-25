import { describe, expect, it } from "vitest";
import {
  priorities,
  priorityLabels,
  priorityTone,
  statuses,
  statusLabels,
  statusTone,
} from "./presentation";

describe("ticket presentation", () => {
  it("provides a distinct label and visual tone for every API status", () => {
    expect(statuses).toEqual(["Open", "InProgress", "Resolved", "Closed"]);
    for (const status of statuses) {
      expect(statusLabels[status]).toBeTruthy();
      expect(statusTone(status)).toBeTruthy();
    }
  });

  it("keeps critical priority visually distinct from low priority", () => {
    expect(priorities).toEqual(["Low", "Medium", "High", "Critical"]);
    expect(priorityLabels.Critical).toBe("Critical");
    expect(priorityTone("Critical")).toBe("red");
    expect(priorityTone("Critical")).not.toBe(priorityTone("Low"));
  });
});
