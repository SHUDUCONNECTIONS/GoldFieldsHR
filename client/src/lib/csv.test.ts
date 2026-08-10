import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { downloadCsv } from "./csv";

describe("downloadCsv", () => {
  let capturedBlobParts: string[] = [];
  let clickSpy: ReturnType<typeof vi.fn>;

  beforeEach(() => {
    capturedBlobParts = [];
    clickSpy = vi.fn();

    vi.stubGlobal(
      "Blob",
      class {
        parts: string[];
        constructor(parts: string[]) {
          this.parts = parts;
          capturedBlobParts = parts;
        }
      },
    );

    URL.createObjectURL = vi.fn(() => "blob:mock-url");
    URL.revokeObjectURL = vi.fn();

    const originalCreateElement = document.createElement.bind(document);
    vi.spyOn(document, "createElement").mockImplementation((tag: string) => {
      const element = originalCreateElement(tag);
      if (tag === "a") {
        (element as HTMLAnchorElement).click = clickSpy as unknown as () => void;
      }
      return element;
    });
  });

  afterEach(() => {
    vi.restoreAllMocks();
    vi.unstubAllGlobals();
  });

  it("joins headers and rows with commas and CRLF", () => {
    downloadCsv("test.csv", ["Name", "Age"], [["Ada", 30]]);

    expect(capturedBlobParts[0]).toBe("Name,Age\r\nAda,30");
  });

  it("quotes values containing commas, quotes, or newlines", () => {
    downloadCsv("test.csv", ["Note"], [['Has "quotes", a comma, and\na newline']]);

    expect(capturedBlobParts[0]).toContain('"Has ""quotes"", a comma, and\na newline"');
  });

  it("renders null and undefined as empty strings", () => {
    downloadCsv("test.csv", ["A", "B"], [[null, undefined]]);

    expect(capturedBlobParts[0]).toBe("A,B\r\n,");
  });

  it("triggers a download via an anchor click", () => {
    downloadCsv("my-export.csv", ["A"], [["1"]]);

    expect(clickSpy).toHaveBeenCalledOnce();
  });
});
