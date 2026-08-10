import { describe, expect, it } from "vitest";
import { formatDuration, formatFileSize } from "./format";

describe("formatFileSize", () => {
  it("formats bytes under 1KB", () => {
    expect(formatFileSize(512)).toBe("512 B");
  });

  it("formats kilobytes", () => {
    expect(formatFileSize(2048)).toBe("2.0 KB");
  });

  it("formats megabytes", () => {
    expect(formatFileSize(5 * 1024 * 1024)).toBe("5.0 MB");
  });

  it("handles zero bytes", () => {
    expect(formatFileSize(0)).toBe("0 B");
  });
});

describe("formatDuration", () => {
  it("returns an em dash for null", () => {
    expect(formatDuration(null)).toBe("—");
  });

  it("formats whole hours", () => {
    expect(formatDuration(2)).toBe("2h 0m");
  });

  it("formats fractional hours as hours and minutes", () => {
    expect(formatDuration(1.5)).toBe("1h 30m");
  });

  it("rounds to the nearest minute", () => {
    expect(formatDuration(0.016)).toBe("0h 1m");
  });

  it("handles zero", () => {
    expect(formatDuration(0)).toBe("0h 0m");
  });
});
