import { test, expect } from '@playwright/test';
import { LoginPage } from '../../../Page objects/Login.page';
import { MyEformsPage } from '../../../Page objects/MyEforms.page';
import { PluginPage } from '../../../Page objects/Plugin.page';

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
    expect(plugin.status).toBe('toggle_off');
  });

  test('should activate the plugin', async () => {
    test.setTimeout(180000);
    const loginPage = new LoginPage(page);
    const myEformsPage = new MyEformsPage(page);
    const pluginPage = new PluginPage(page);

    const plugin = await pluginPage.getFirstPluginRowObj();
    await plugin.enableOrDisablePlugin();

    // After enableOrDisablePlugin: re-logged in and on plugins page
    const pluginAfter = await pluginPage.getFirstPluginRowObj();
    expect(pluginAfter.id).toBe(1);
    expect(pluginAfter.name).toBe('Microting Workflow Plugin');
    expect(pluginAfter.status).toBe('toggle_on');
  });
});
