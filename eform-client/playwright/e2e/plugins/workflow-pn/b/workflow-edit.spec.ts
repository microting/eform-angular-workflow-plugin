import { test, expect } from '@playwright/test';
import { LoginPage } from '../../../../Page objects/Login.page';
import { WorkflowCasesPage, WorkflowCaseForEdit } from '../WorkflowCases.page';
import { generateRandmString } from '../../../../helper-functions';
import { format } from 'date-fns';

let page;

test.describe('Workflow cases - Edit', () => {
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

  test('should not edit workflow case', async () => {
    const workflowCasesPage = new WorkflowCasesPage(page);
    const modelForUpdate = new WorkflowCaseForEdit();
    const dateNow = new Date();
    modelForUpdate.status = 'Igangværende';
    modelForUpdate.actionPlan = generateRandmString();
    modelForUpdate.description = generateRandmString();
    modelForUpdate.deadline = { day: dateNow.getDate(), month: dateNow.getMonth(), year: dateNow.getFullYear() };
    modelForUpdate.dateOfIncident = { day: dateNow.getDate(), month: dateNow.getMonth(), year: dateNow.getFullYear() };
    const firstWorkflowCase = await workflowCasesPage.getFirstWorkflowCase();
    await firstWorkflowCase.update(modelForUpdate, true);
    const findWorkflowCase = await workflowCasesPage.getFirstWorkflowCase();
    expect(findWorkflowCase.status).toBe('Vælg status');
    expect(findWorkflowCase.dateOfIncident).toBe(firstWorkflowCase.dateOfIncident);
    expect(findWorkflowCase.deadline).toBe(firstWorkflowCase.deadline);
    expect(findWorkflowCase.description).toBe(firstWorkflowCase.description);
    expect(findWorkflowCase.actionPlan).toBe(firstWorkflowCase.actionPlan);
  });

  test('should edit workflow case', async () => {
    const workflowCasesPage = new WorkflowCasesPage(page);
    const modelForUpdate = new WorkflowCaseForEdit();
    const dateNow = new Date();
    modelForUpdate.status = 'Igangværende';
    modelForUpdate.actionPlan = generateRandmString();
    modelForUpdate.description = generateRandmString();
    modelForUpdate.deadline = { day: dateNow.getDate(), month: dateNow.getMonth(), year: dateNow.getFullYear() };
    modelForUpdate.dateOfIncident = { day: dateNow.getDate(), month: dateNow.getMonth(), year: dateNow.getFullYear() };
    const firstWorkflowCase = await workflowCasesPage.getFirstWorkflowCase();
    await firstWorkflowCase.update(modelForUpdate);
    const findWorkflowCase = await workflowCasesPage.getFirstWorkflowCase();
    expect(findWorkflowCase.id).toBe(1);
    expect(findWorkflowCase.status).toBe(modelForUpdate.status);

    const dateOfIncident = new Date();
    dateOfIncident.setFullYear(
      modelForUpdate.dateOfIncident.year,
      modelForUpdate.dateOfIncident.month,
      modelForUpdate.dateOfIncident.day,
    );

    const deadline = new Date();
    deadline.setFullYear(
      modelForUpdate.deadline.year,
      modelForUpdate.deadline.month,
      modelForUpdate.deadline.day,
    );

    expect(findWorkflowCase.dateOfIncident).toBe(format(dateOfIncident, 'dd.MM.yyyy'));
    expect(findWorkflowCase.deadline).toBe(format(deadline, 'dd.MM.yyyy'));
    expect(findWorkflowCase.description).toBe(modelForUpdate.description);
    expect(findWorkflowCase.actionPlan).toBe(modelForUpdate.actionPlan);
  });
});
