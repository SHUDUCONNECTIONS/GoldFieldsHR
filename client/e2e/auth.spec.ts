import { expect, test } from "@playwright/test";

test.describe("Authentication", () => {
  test("shows an error for invalid credentials", async ({ page }) => {
    await page.goto("/login");
    await page.getByLabel("Email").fill("nobody@example.com");
    await page.getByLabel("Password").fill("wrong-password");
    await page.getByRole("button", { name: "Sign in" }).click();

    await expect(page.getByText(/invalid|incorrect|unauthorized/i)).toBeVisible();
    await expect(page).toHaveURL(/\/login$/);
  });

  test("logs in with valid credentials and can log out", async ({ page }) => {
    await page.goto("/login");
    await page.getByLabel("Email").fill("hr.admin@goldfieldshr.local");
    await page.getByLabel("Password").fill("Bootstrap@123");
    await page.getByRole("button", { name: "Sign in" }).click();

    await page.waitForURL("http://localhost:5173/");
    await expect(page.getByText("HR Admin")).toBeVisible();

    await page.getByRole("button", { name: "Sign out" }).click();
    await page.waitForURL("**/login");
  });
});
