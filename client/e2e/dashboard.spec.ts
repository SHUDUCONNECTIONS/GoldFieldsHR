import { expect, test } from "@playwright/test";

test("dashboard loads with module launch grid after login", async ({ page }) => {
  await page.goto("/login");
  await page.getByLabel("Email").fill("hr.admin@goldfieldshr.local");
  await page.getByLabel("Password").fill("Bootstrap@123");
  await page.getByRole("button", { name: "Sign in" }).click();
  await page.waitForURL("http://localhost:5173/");

  await expect(page.getByText(/Welcome back,/)).toBeVisible();
  await expect(page.getByText("Attendance Today")).toBeVisible();
  await expect(page.getByText("Incidents (MTD)")).toBeVisible();
  await expect(page.getByText("Attendance overview")).toBeVisible();
});
