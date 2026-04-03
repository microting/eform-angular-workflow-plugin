import { test, expect } from '@playwright/test';
import { LoginPage } from '../../../Page objects/Login.page';
import { WorkflowCasesPage } from '../WorkflowCases.page';

let page;

test.describe('Workflow cases - Filtration', () => {
  test.beforeAll(async ({ browser }) => {
    page = await browser.newPage();
    const loginPage = new LoginPage(page);
    await loginPage.open('/auth');
    await loginPage.login();
    const workflowCasesPage = new WorkflowCasesPage(page);
    await workflowCasesPage.goToWorkflowCasesPage();
  });

  test.afterAll(async () => {
    await page.close();
  });

  test('should be able to filtration of all workflow cases', async () => {
    const workflowCasesPage = new WorkflowCasesPage(page);
    await workflowCasesPage.searchInput().fill('9971c397-61a9-4b6d-aa68-38bf30123360');
    await page.waitForTimeout(2000);
    await page.locator('#spinner-animation').waitFor({ state: 'hidden', timeout: 90000 }).catch(() => {});
    const findWorkflowCase = await workflowCasesPage.getFirstWorkflowCase();
    expect(findWorkflowCase.id).toBe(3);
    expect(findWorkflowCase.dateOfIncident).toBe('26.06.2021');
    expect(findWorkflowCase.incidentType).toBe('9971c397-61a9-4b6d-aa68-38bf30123360');
    expect(findWorkflowCase.incidentPlace).toBe('d23fa495-e8d0-4545-840d-62446f16fe99');
    expect(findWorkflowCase.description).toBe('b4068568-4bc5-405a-a92f-7d4a3080fc6b');
    expect(findWorkflowCase.deadline).toBe('01.07.2021');
    expect(findWorkflowCase.actionPlan).toBe('591f85e9-bd96-4ed2-9041-ce6d335e79fb');
    expect(findWorkflowCase.toBeSolvedBy).toBe('--');
    expect(findWorkflowCase.status).toBe('Igangværende');
  });
});
