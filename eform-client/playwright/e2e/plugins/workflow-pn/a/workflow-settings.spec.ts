import { test, expect } from '@playwright/test';
import { LoginPage } from '../../../../Page objects/Login.page';
import { MyEformsPage } from '../../../../Page objects/MyEforms.page';
import { PluginPage } from '../../../../Page objects/Plugin.page';

let page;

test.describe('Application settings page - site header section', () => {
  test.beforeAll(async ({ browser }) => {
    page = await browser.newPage();
    const loginPage = new LoginPage(page);
    await loginPage.open('/auth');
  });

  test.afterAll(async () => {
    await page.close();
  });

  test('should go to plugin settings page', async () => {
    const loginPage = new LoginPage(page);
    const myEformsPage = new MyEformsPage(page);
    const pluginPage = new PluginPage(page);

    await loginPage.login();
    await myEformsPage.Navbar.goToPluginsPage();

    const plugin = await pluginPage.getFirstPluginRowObj();
    expect(plugin.id).toBe(1);
    expect(plugin.name).toBe('Microting Workflow Plugin');

    // Open action menu to check status
    const actionMenuBtn = page.locator('#action-items-0').locator('#actionMenu');
    await actionMenuBtn.waitFor({ state: 'visible', timeout: 40000 });
    await actionMenuBtn.click();
    await page.waitForTimeout(500);

    const statusBtn = page.locator('#plugin-status-button0');
    await statusBtn.waitFor({ state: 'visible', timeout: 40000 });
    const statusIcon = statusBtn.locator('mat-icon');
    const status = await statusIcon.textContent();
    expect(status).toBe('toggle_off');

    // Close the menu
    await page.keyboard.press('Escape');
    await page.waitForTimeout(300);
  });

  test('should activate the plugin', async () => {
    const loginPage = new LoginPage(page);
    const myEformsPage = new MyEformsPage(page);
    const pluginPage = new PluginPage(page);

    // Open action menu
    const actionMenuBtn = page.locator('#action-items-0').locator('#actionMenu');
    await actionMenuBtn.waitFor({ state: 'visible', timeout: 40000 });
    await actionMenuBtn.click();
    await page.waitForTimeout(500);

    // Click on the status button inside the menu
    const statusBtn = page.locator('#plugin-status-button0');
    await statusBtn.waitFor({ state: 'visible', timeout: 40000 });
    await statusBtn.click();
    await page.waitForTimeout(500);

    // Confirm activation in the modal
    const pluginOKBtn = page.locator('#pluginOKBtn');
    await pluginOKBtn.waitFor({ state: 'visible', timeout: 40000 });
    await pluginOKBtn.click();
    await page.waitForTimeout(100000); // Wait for plugin to create db etc.

    // Re-login and navigate back to plugins page
    await loginPage.open('/');
    await loginPage.login();
    await myEformsPage.Navbar.goToPluginsPage();
    await page.waitForTimeout(500);

    // Verify the plugin is now activated
    const plugin = await pluginPage.getFirstPluginRowObj();
    expect(plugin.id).toBe(1);
    expect(plugin.name).toBe('Microting Workflow Plugin');

    // Open action menu to check new status
    const actionMenuBtn2 = page.locator('#action-items-0').locator('#actionMenu');
    await actionMenuBtn2.waitFor({ state: 'visible', timeout: 40000 });
    await actionMenuBtn2.click();
    await page.waitForTimeout(500);

    const statusBtn2 = page.locator('#plugin-status-button0');
    await statusBtn2.waitFor({ state: 'visible', timeout: 40000 });
    const statusIcon2 = statusBtn2.locator('mat-icon');
    const status = await statusIcon2.textContent();
    expect(status).toBe('toggle_on');
  });
});
