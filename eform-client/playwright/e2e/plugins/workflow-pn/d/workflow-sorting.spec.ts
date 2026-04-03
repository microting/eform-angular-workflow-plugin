import { test } from '@playwright/test';
import { LoginPage } from '../../../../Page objects/Login.page';
import { WorkflowCasesPage } from '../WorkflowCases.page';
import { testSorting } from '../../../../helper-functions';

let page;

test.describe('Workflow cases - Sorting', () => {
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

  test('should be able to sort by ID', async () => {
    await testSorting(page, 'thead > tr > th.mat-column-id', '#workflowCaseId', 'ID');
  });

  test('should be able to sort by date of incident', async () => {
    await testSorting(
      page,
      'thead > tr > th.mat-column-dateOfIncident',
      '#workflowCaseDateOfIncident',
      'date of incident',
    );
  });

  test('should be able to sort by incident type', async () => {
    await testSorting(
      page,
      'thead > tr > th.mat-column-incidentType',
      '#workflowCaseIncidentType',
      'incident type',
    );
  });

  test('should be able to sort by incident place', async () => {
    await testSorting(
      page,
      'thead > tr > th.mat-column-incidentPlace',
      '#workflowCaseIncidentPlace',
      'incident place',
    );
  });

  test('should be able to sort by description', async () => {
    await testSorting(
      page,
      'thead > tr > th.mat-column-description',
      '#workflowCaseDescription',
      'description',
    );
  });

  test('should be able to sort by action plan', async () => {
    await testSorting(
      page,
      'thead > tr > th.mat-column-actionPlan',
      '#workflowCaseActionPlan',
      'action plan',
    );
  });
});
