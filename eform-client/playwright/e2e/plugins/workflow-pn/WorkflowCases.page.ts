import { Page, Locator } from '@playwright/test';
import { PageWithNavbarPage } from '../../Page objects/PageWithNavbar.page';
import { selectDateOnNewDatePicker } from '../../helper-functions';

export class WorkflowCasesPage extends PageWithNavbarPage {
  constructor(page: Page) {
    super(page);
  }

  public async rowNum(): Promise<number> {
    await this.page.waitForTimeout(500);
    return await this.page.locator('#tableBody > tr').count();
  }

  public idTableHeader(): Locator {
    return this.page.locator('thead > tr > th.mat-column-id');
  }

  public dateOfIncidentHeader(): Locator {
    return this.page.locator('thead > tr > th.mat-column-dateOfIncident');
  }

  public incidentTypeHeader(): Locator {
    return this.page.locator('thead > tr > th.mat-column-incidentType');
  }

  public incidentPlaceHeader(): Locator {
    return this.page.locator('thead > tr > th.mat-column-incidentPlace');
  }

  public photosExistsHeader(): Locator {
    return this.page.locator('#photosExistsHeader');
  }

  public descriptionHeader(): Locator {
    return this.page.locator('thead > tr > th.mat-column-description');
  }

  public deadlineHeader(): Locator {
    return this.page.locator('#deadlineHeader');
  }

  public actionPlanHeader(): Locator {
    return this.page.locator('thead > tr > th.mat-column-actionPlan');
  }

  public searchInput(): Locator {
    return this.page.locator('#searchInput');
  }

  public saveEditBtn(): Locator {
    return this.page.locator('#saveEditBtn');
  }

  public cancelEditBtn(): Locator {
    return this.page.locator('#cancelEditBtn');
  }

  public toBeSolvedByEdit(): Locator {
    return this.page.locator('#toBeSolvedByEdit');
  }

  public statusEdit(): Locator {
    return this.page.locator('#statusEdit');
  }

  public workflowCaseDeleteDeleteBtn(): Locator {
    return this.page.locator('#workflowCaseDeleteDeleteBtn');
  }

  public workflowCaseDeleteCancelBtn(): Locator {
    return this.page.locator('#workflowCaseDeleteCancelBtn');
  }

  public workflowPnCases(): Locator {
    return this.page.locator('#workflow-pn-cases');
  }

  public workflowPn(): Locator {
    return this.page.locator('#workflow-pn');
  }

  public deadlineFormInput(): Locator {
    return this.page.locator('#deadline');
  }

  public dateOfIncidentFormInput(): Locator {
    return this.page.locator('#dateOfIncident');
  }

  public descriptionEdit(): Locator {
    return this.page.locator('#descriptionEdit');
  }

  public actionPlanEdit(): Locator {
    return this.page.locator('#actionPlanEdit');
  }

  public async goToWorkflowCasesPage(): Promise<void> {
    await this.workflowPn().click();
    await this.workflowPnCases().click();
    await this.page.locator('#spinner-animation').waitFor({ state: 'hidden', timeout: 90000 }).catch(() => {});
    await this.searchInput().waitFor({ state: 'visible', timeout: 90000 });
  }

  public async getFirstWorkflowCase(): Promise<WorkflowCaseRowObject> {
    return this.getWorkflowCaseByNumber(1);
  }

  public async getWorkflowCaseByNumber(number: number): Promise<WorkflowCaseRowObject> {
    const obj = new WorkflowCaseRowObject(this.page, this);
    return await obj.getRow(number);
  }
}

export class WorkflowCaseRowObject {
  public page: Page;
  public workflowCasesPage: WorkflowCasesPage;

  constructor(page: Page, workflowCasesPage: WorkflowCasesPage) {
    this.page = page;
    this.workflowCasesPage = workflowCasesPage;
  }

  public id: number;
  public dateOfIncident: string;
  public incidentType: string;
  public incidentPlace: string;
  public description: string;
  public deadline: string;
  public actionPlan: string;
  public toBeSolvedBy: string;
  public status: string;
  public menuBtn: Locator;
  public updateBtn: Locator;
  public deleteBtn: Locator;

  public async closeEdit(clickCancel = false): Promise<void> {
    if (!clickCancel) {
      await this.workflowCasesPage.saveEditBtn().click();
      await this.page.locator('#spinner-animation').waitFor({ state: 'hidden', timeout: 90000 }).catch(() => {});
    } else {
      await this.workflowCasesPage.cancelEditBtn().click();
    }
    await this.page.waitForTimeout(500);
    await this.workflowCasesPage.searchInput().waitFor({ state: 'visible' });
    await this.page.waitForTimeout(500);
  }

  public async closeDelete(clickCancel = false): Promise<void> {
    if (!clickCancel) {
      await this.workflowCasesPage.workflowCaseDeleteDeleteBtn().click();
      await this.page.locator('#spinner-animation').waitFor({ state: 'hidden', timeout: 90000 }).catch(() => {});
    } else {
      await this.workflowCasesPage.workflowCaseDeleteCancelBtn().click();
    }
    await this.page.waitForTimeout(500);
    await this.workflowCasesPage.searchInput().waitFor({ state: 'visible' });
    await this.page.waitForTimeout(500);
  }

  async getRow(rowNum: number): Promise<WorkflowCaseRowObject> {
    const rowIndex = rowNum - 1;
    const rows = this.page.locator('table tr.mat-mdc-row');
    if ((await rows.count()) >= rowNum) {
      this.id = +(await this.page.locator('tbody > tr > td.mat-column-id').nth(rowIndex).textContent() || '0').trim();
      this.dateOfIncident = (await this.page.locator('tbody > tr > td.mat-column-dateOfIncident').nth(rowIndex).textContent() || '').trim();
      this.incidentType = (await this.page.locator('tbody > tr > td.mat-column-incidentType').nth(rowIndex).textContent() || '').trim();
      this.incidentPlace = (await this.page.locator('tbody > tr > td.mat-column-incidentPlace').nth(rowIndex).textContent() || '').trim();
      this.description = (await this.page.locator('tbody > tr > td.mat-column-description').nth(rowIndex).textContent() || '').trim();
      this.deadline = (await this.page.locator('tbody > tr > td.mat-column-deadline').nth(rowIndex).textContent() || '').trim();
      this.actionPlan = (await this.page.locator('tbody > tr > td.mat-column-actionPlan').nth(rowIndex).textContent() || '').trim();
      this.toBeSolvedBy = (await this.page.locator('tbody > tr > td.mat-column-solvedBy').nth(rowIndex).textContent() || '').trim();
      this.status = (await this.page.locator('tbody > tr > td.mat-column-status').nth(rowIndex).textContent() || '').trim();
      this.menuBtn = this.page.locator(`#actionMenu-${rowIndex}`);
      this.updateBtn = this.page.locator(`#editWorkflowCaseBtn-${rowIndex}`);
      this.deleteBtn = this.page.locator(`#deleteBtn-${rowIndex}`);
    }
    return this;
  }

  public async openDelete(): Promise<void> {
    await this.menuBtn.click();
    await this.page.waitForTimeout(500);
    await this.deleteBtn.click();
    await this.workflowCasesPage.workflowCaseDeleteCancelBtn().waitFor({ state: 'visible', timeout: 20000 });
  }

  public async openEdit(updateModel: WorkflowCaseForEdit): Promise<void> {
    await this.menuBtn.click();
    await this.page.waitForTimeout(500);
    await this.updateBtn.click();
    await this.workflowCasesPage.cancelEditBtn().waitFor({ state: 'visible', timeout: 20000 });
    if (updateModel) {
      if (updateModel.status) {
        await this.workflowCasesPage.statusEdit().locator('input').fill(updateModel.status);
        await this.page.waitForTimeout(1000);
        const option = this.page.locator('ng-dropdown-panel').locator('.ng-option').filter({ hasText: updateModel.status }).first();
        await option.waitFor({ state: 'visible', timeout: 20000 });
        await option.click();
      }
      if (updateModel.deadline) {
        await this.workflowCasesPage.deadlineFormInput().click();
        await selectDateOnNewDatePicker(this.page, updateModel.deadline.year, updateModel.deadline.month, updateModel.deadline.day);
      }
      if (updateModel.dateOfIncident) {
        await this.workflowCasesPage.dateOfIncidentFormInput().click();
        await selectDateOnNewDatePicker(this.page, updateModel.dateOfIncident.year, updateModel.dateOfIncident.month, updateModel.dateOfIncident.day);
      }
      if (updateModel.description) {
        await this.workflowCasesPage.descriptionEdit().fill(updateModel.description);
      }
      if (updateModel.actionPlan) {
        await this.workflowCasesPage.actionPlanEdit().fill(updateModel.actionPlan);
      }
    }
  }

  async update(updateModel: WorkflowCaseForEdit, clickCancel = false): Promise<void> {
    await this.openEdit(updateModel);
    await this.closeEdit(clickCancel);
  }

  async delete(clickCancel = false): Promise<void> {
    await this.openDelete();
    await this.closeDelete(clickCancel);
  }
}

export class WorkflowCaseForEdit {
  public dateOfIncident: {
    year: number;
    month: number;
    day: number;
  };
  public description: string;
  public deadline: {
    year: number;
    month: number;
    day: number;
  };
  public actionPlan: string;
  public status: string;
}
