import Page from '../Page';
import { parse } from 'date-fns';

export class WorkflowCasesPage extends Page {
  constructor() {
    super();
  }

  public get rowNum(): number {
    browser.pause(500);
    return $$('#tableBody > tr').length;
  }

  public goToWorkflowCasesPage() {
    this.workflowPn.click();
    this.workflowPnCases.click();
    $('#spinner-animation').waitForDisplayed({ timeout: 90000, reverse: true });
    this.searchInput.waitForClickable({ timeout: 90000 });
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
    ele.waitForDisplayed({ timeout: 40000 });
    // ele.waitForClickable({ timeout: 40000 });
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
      this.dateOfIncident = parse(date, 'dd.MM.yyyy HH:mm:ss', new Date());
      date = row.$('#workflowCaseUpdatedAt').getText();
      this.updateAt = parse(date, 'dd.MM.yyyy HH:mm:ss', new Date());
      this.incidentType = row.$('#workflowCaseIncidentType').getText();
      this.incidentPlace = row.$('#workflowCaseIncidentPlace').getText();
      this.photo = row.$('#workflowCasePhotosExists').getText() === 'true';
      this.description = row.$('#workflowCaseDescription').getText();
      date = row.$('#workflowCaseDeadline').getText();
      this.deadline = parse(date, 'dd.MM.yyyy HH:mm:ss', new Date());
      this.actionPlan = row.$('#workflowCaseActionPlan').getText();
      this.toBeSolvedBy = row.$('#workflowCaseToBeSolvedBy').getText();
      this.status = row.$('#workflowCaseStatus').getText();
      this.updateBtn = row.$('#editWorkflowCaseBtn');
      this.deleteBtn = row.$('#deleteBtn');
    }
  }

  public id: number;
  public dateOfIncident: Date;
  public updateAt: Date;
  public incidentType: string;
  public incidentPlace: string;
  public photo: boolean;
  public description: string;
  public deadline: Date;
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

  public openEdit() {
    this.updateBtn.click();
    $('#spinner-animation').waitForDisplayed({ timeout: 90000, reverse: true });
    workflowCasesPage.cancelEditBtn.waitForDisplayed({
      timeout: 20000,
    });
  }

  update(toBeSolvedBy: string, status: string, clickCancel = false) {
    this.openEdit();
    const spinnerAnimation = $('#spinner-animation');
    const ngOption = $('.ng-option');
    if (status) {
      workflowCasesPage.statusEdit.$('input').setValue(status);
      spinnerAnimation.waitForDisplayed({ timeout: 90000, reverse: true });
      ngOption.waitForDisplayed({ timeout: 20000 });
      workflowCasesPage.statusEdit
        .$('.ng-dropdown-panel')
        .$(`.ng-option=${status}`)
        .click();
    }
    if (toBeSolvedBy) {
      workflowCasesPage.toBeSolvedByEdit.$('input').setValue(toBeSolvedBy);
      spinnerAnimation.waitForDisplayed({ timeout: 90000, reverse: true });
      ngOption.waitForDisplayed({ timeout: 20000 });
      workflowCasesPage.toBeSolvedByEdit
        .$('.ng-dropdown-panel')
        .$(`.ng-option=${toBeSolvedBy}`)
        .click();
    }
    WorkflowCaseRowObject.closeEdit(clickCancel);
  }

  delete(clickCancel = false) {
    this.openDelete();
    WorkflowCaseRowObject.closeDelete(clickCancel);
  }
}
