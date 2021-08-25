import Page from '../Page';
import { parse, format } from 'date-fns';

export class WorkflowCasesPage extends Page {
  constructor() {
    super();
  }

  public get rowNum(): number {
    browser.pause(500);
    return $$('#tableBody > tr').length;
  }

  public get idTableHeader() {
    const ele = $('#idTableHeader');
    ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get dateOfIncidentHeader() {
    const ele = $('#dateOfIncidentHeader');
    ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get incidentTypeHeader() {
    const ele = $('#incidentTypeHeader');
    ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get incidentPlaceHeader() {
    const ele = $('#incidentPlaceHeader');
    ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get photosExistsHeader() {
    const ele = $('#photosExistsHeader');
    ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get descriptionHeader() {
    const ele = $('#descriptionHeader');
    ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get deadlineHeader() {
    const ele = $('#deadlineHeader');
    ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get actionPlanHeader() {
    const ele = $('#actionPlanHeader');
    ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get searchInput() {
    const ele = $('#searchInput');
    ele.waitForDisplayed({ timeout: 40000 });
    // ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get saveEditBtn() {
    const ele = $('#saveEditBtn');
    ele.waitForDisplayed({ timeout: 40000 });
    // ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get cancelEditBtn() {
    const ele = $('#cancelEditBtn');
    ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get toBeSolvedByEdit() {
    const ele = $('#toBeSolvedByEdit');
    ele.waitForDisplayed({ timeout: 40000 });
    // ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get statusEdit() {
    const ele = $('#statusEdit');
    // ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get workflowCaseDeleteDeleteBtn() {
    const ele = $('#workflowCaseDeleteDeleteBtn');
    ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get workflowCaseDeleteCancelBtn() {
    const ele = $('#workflowCaseDeleteCancelBtn');
    ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get workflowPnCases() {
    const ele = $('#workflow-pn-cases');
    ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get workflowPn() {
    const ele = $('#workflow-pn');
    ele.waitForDisplayed({ timeout: 40000 });
    ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get deadlineFormInput() {
    const ele = $('#deadline');
    ele.waitForDisplayed({ timeout: 40000 });
    // ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get dateOfIncidentFormInput() {
    const ele = $('#dateOfIncident');
    ele.waitForDisplayed({ timeout: 40000 });
    // ele.waitForClickable({ timeout: 40000 });
    return ele;
  }

  public get descriptionEdit() {
    const ele = $('#descriptionEdit');
    ele.waitForDisplayed({ timeout: 40000 });
    // ele.waitForClickable({ timeout: 40000 });
    // ele = $('#descriptionEdit .pell-content');
    return ele;
  }

  public get actionPlanEdit() {
    const ele = $('#actionPlanEdit');
    ele.waitForDisplayed({ timeout: 40000 });
    // ele.waitForClickable({ timeout: 40000 });
    // ele = $('#actionPlanEdit .pell-content');
    return ele;
  }

  public goToWorkflowCasesPage() {
    this.workflowPn.click();
    this.workflowPnCases.click();
    $('#spinner-animation').waitForDisplayed({ timeout: 90000, reverse: true });
    this.searchInput.waitForClickable({ timeout: 90000 });
  }

  public getFirstWorkflowCase(): WorkflowCaseRowObject {
    return this.getWorkflowCaseByNumber(1);
  }

  public getWorkflowCaseByNumber(number: number): WorkflowCaseRowObject {
    return new WorkflowCaseRowObject(number);
  }
}

const workflowCasesPage = new WorkflowCasesPage();
export default workflowCasesPage;

export class WorkflowCaseRowObject {
  constructor(rowNumber) {
    const row = $$('#tableBody tr')[rowNumber - 1];
    if (row) {
      this.id = +row.$('#workflowCaseId').getText();
      let date = row.$('#workflowCaseDateOfIncident').getText();
      this.dateOfIncident = date; // parse(date, 'dd.MM.yyyy HH:mm:ss', new Date());
      // date = row.$('#workflowCaseUpdatedAt').getText();
      // this.updateAt = parse(date, 'dd.MM.yyyy HH:mm:ss', new Date());
      this.incidentType = row.$('#workflowCaseIncidentType').getText();
      this.incidentPlace = row.$('#workflowCaseIncidentPlace').getText();
      // this.photo = row.$('#workflowCasePhotosExists').getText() === 'true';
      this.description = row.$('#workflowCaseDescription').getText();
      date = row.$('#workflowCaseDeadline').getText();
      this.deadline = date; // parse(date, 'dd.MM.yyyy HH:mm:ss', new Date());
      this.actionPlan = row.$('#workflowCaseActionPlan').getText();
      this.toBeSolvedBy = row.$('#workflowCaseToBeSolvedBy').getText();
      this.status = row.$('#workflowCaseStatus').getText();
      this.updateBtn = row.$('#editWorkflowCaseBtn');
      this.deleteBtn = row.$('#deleteBtn');
    }
  }

  public id: number;
  public dateOfIncident: string;
  // public updateAt: Date;
  public incidentType: string;
  public incidentPlace: string;
  // public photo: boolean;
  public description: string;
  public deadline: string;
  public actionPlan: string;
  public toBeSolvedBy: string;
  public status: string;
  public updateBtn: WebdriverIO.Element;
  public deleteBtn: WebdriverIO.Element;

  public static closeEdit(clickCancel = false) {
    if (!clickCancel) {
      workflowCasesPage.saveEditBtn.click();
      $('#spinner-animation').waitForDisplayed({
        timeout: 90000,
        reverse: true,
      });
    } else {
      workflowCasesPage.cancelEditBtn.click();
    }
    workflowCasesPage.searchInput.waitForDisplayed();
    browser.pause(500);
  }

  public static closeDelete(clickCancel = false) {
    if (!clickCancel) {
      workflowCasesPage.workflowCaseDeleteDeleteBtn.click();
      $('#spinner-animation').waitForDisplayed({
        timeout: 90000,
        reverse: true,
      });
    } else {
      workflowCasesPage.workflowCaseDeleteCancelBtn.click();
    }
    workflowCasesPage.searchInput.waitForDisplayed();
  }

  public openDelete() {
    this.deleteBtn.waitForClickable({ timeout: 20000 });
    this.deleteBtn.click();
    workflowCasesPage.workflowCaseDeleteCancelBtn.waitForDisplayed({
      timeout: 20000,
    });
  }

  public openEdit(updateModel: WorkflowCaseForEdit) {
    this.updateBtn.click();
    const spinnerAnimation = $('#spinner-animation');
    spinnerAnimation.waitForDisplayed({ timeout: 90000, reverse: true });
    workflowCasesPage.cancelEditBtn.waitForDisplayed({
      timeout: 20000,
    });
    if (updateModel) {
      const ngOption = $('.ng-option-label');
      if (updateModel.status) {
        workflowCasesPage.statusEdit.$('input').setValue(updateModel.status);
        // spinnerAnimation.waitForDisplayed({ timeout: 90000, reverse: true });
        // ngOption.waitForDisplayed({ timeout: 20000 });
        browser.pause(1000);
        const ele = $(`//*[@id="statusEdit"]//*[text()="${updateModel.status}"]`);
        ele.waitForDisplayed({timeout: 20000});
        ele.click();

        // workflowCasesPage.statusEdit
        //   .$('.ng-dropdown-panel')
        //   .$(`.ng-option-label`)
        //   .click();
      }
      // if (updateModel.toBeSolvedBy) {
      //   workflowCasesPage.toBeSolvedByEdit
      //     .$('input')
      //     .setValue(updateModel.toBeSolvedBy);
      //   spinnerAnimation.waitForDisplayed({ timeout: 90000, reverse: true });
      //   ngOption.waitForDisplayed({ timeout: 20000 });
      //   workflowCasesPage.toBeSolvedByEdit
      //     .$('.ng-dropdown-panel')
      //     .$(`.ng-option=${updateModel.toBeSolvedBy}`)
      //     .click();
      // }
      if (updateModel.deadline) {
        workflowCasesPage.deadlineFormInput.setValue(
          format(updateModel.deadline, 'MM.dd.yyyy')
        );
      }
      if (updateModel.dateOfIncident) {
        workflowCasesPage.dateOfIncidentFormInput.setValue(
          format(updateModel.dateOfIncident, 'MM.dd.yyyy')
        );
      }
      if (updateModel.description) {
        workflowCasesPage.descriptionEdit.setValue(updateModel.description);
      }
      if (updateModel.actionPlan) {
        workflowCasesPage.actionPlanEdit.setValue(updateModel.actionPlan);
      }
    }
  }

  update(updateModel: WorkflowCaseForEdit, clickCancel = false) {
    this.openEdit(updateModel);
    WorkflowCaseRowObject.closeEdit(clickCancel);
  }

  delete(clickCancel = false) {
    this.openDelete();
    WorkflowCaseRowObject.closeDelete(clickCancel);
  }
}

export class WorkflowCaseForEdit {
  public dateOfIncident: Date;
  // public incidentPlace: string;
  public description: string;
  public deadline: Date;
  public actionPlan: string;
  // public toBeSolvedBy: string;
  public status: string;
}
