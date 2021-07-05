import loginPage from '../../Page objects/Login.page';
import workflowCasesPage from '../../Page objects/Workflow/WorkflowCases.page';
import { testSorting } from '../../Helpers/helper-functions';
// import { parse } from 'date-fns';

describe('Workflow cases - Sorting', function () {
  before(function () {
    loginPage.open('/auth');
    loginPage.login();
    workflowCasesPage.goToWorkflowCasesPage();
  });
  it('should be able to sort by ID', function () {
    testSorting(workflowCasesPage.idTableHeader, '#workflowCaseId', 'ID');
  });
  it('should be able to sort by date of incident', function () {
    testSorting(
      workflowCasesPage.dateOfIncidentHeader,
      '#workflowCaseDateOfIncident',
      'date of incident'
    );
  });
  it('should be able to sort by incident type', function () {
    testSorting(
      workflowCasesPage.incidentTypeHeader,
      '#workflowCaseIncidentType',
      'incident type'
    );
  });
  it('should be able to sort by incident place', function () {
    testSorting(
      workflowCasesPage.incidentPlaceHeader,
      '#workflowCaseIncidentPlace',
      'incident place'
    );
  });
  it('should be able to sort by photos exists', function () {
    testSorting(
      workflowCasesPage.photosExistsHeader,
      '#workflowCasePhotosExists',
      'photos exists'
    );
  });
  it('should be able to sort by description', function () {
    testSorting(
      workflowCasesPage.descriptionHeader,
      '#workflowCaseDescription',
      'description'
    );
  });
  // it('should be able to sort by deadline', function () {
  //   testSorting(
  //     workflowCasesPage.deadlineHeader,
  //     '#workflowCaseDeadline',
  //     'deadline',
  //     (ele) => parse(ele.getText(), 'dd.MM.yyyy HH:mm:ss', new Date())
  //   );
  // });
  it('should be able to sort by action plan', function () {
    testSorting(
      workflowCasesPage.actionPlanHeader,
      '#workflowCaseActionPlan',
      'action plan'
    );
  });
});
