import { expect, test } from "@playwright/test";

test.describe("Employee directory", () => {
  test.beforeEach(async ({ page }) => {
    await page.goto("/login");
    await page.getByLabel("Email").fill("hr.admin@goldfieldshr.local");
    await page.getByLabel("Password").fill("Bootstrap@123");
    await page.getByRole("button", { name: "Sign in" }).click();
    await page.waitForURL("http://localhost:5173/");

    await page.goto("/settings");
    await page.waitForSelector("text=Employee directory");
  });

  function directory(page: import("@playwright/test").Page) {
    return page.locator("div.rounded-lg", { hasText: "Employee directory" });
  }

  test("lists employees with working pagination", async ({ page }) => {
    const dir = directory(page);
    await expect(dir.getByText(/Showing \d+/)).toBeVisible();

    const rowCountPage1 = await dir.locator("table tbody tr").count();
    expect(rowCountPage1).toBeGreaterThan(0);

    const nextButton = dir.getByRole("button", { name: "Next" });
    if (await nextButton.isEnabled()) {
      await nextButton.click();
      await expect(dir.getByText(/Page 2 of/)).toBeVisible();
    }
  });

  test("search narrows results to matching employees", async ({ page }) => {
    const dir = directory(page);
    await dir.getByPlaceholder("Search name, #, email, title...").fill("hr.admin");

    await expect(dir.getByText("hr.admin@goldfieldshr.local")).toBeVisible();
    await expect(dir.getByText(/Showing 1.1 of 1/)).toBeVisible();
  });
});
