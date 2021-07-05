import loginPage from '../../Page objects/Login.page';
import workflowCasesPage from '../../Page objects/Workflow/WorkflowCases.page';
import { expect } from 'chai';
import { format } from 'date-fns';

describe('Workflow cases - Edit', function () {
  before(function () {
    loginPage.open('/auth');
    loginPage.login();
    workflowCasesPage.goToWorkflowCasesPage();
  });
  it('should not edit status workflow case', function () {
    const firstWorkflowCase = workflowCasesPage.getFirstWorkflowCase();
    firstWorkflowCase.update('', 'Ongoing', true);
    const findWorkflowCase = workflowCasesPage.getFirstWorkflowCase();
    expect(findWorkflowCase.id).equal(1);
    expect(findWorkflowCase.status, 'status has been updated').equal(
      firstWorkflowCase.status
    );
  });
  it('should edit status workflow case', function () {
    const firstWorkflowCase = workflowCasesPage.getFirstWorkflowCase();
    firstWorkflowCase.update('', 'Ongoing');
    const findWorkflowCase = workflowCasesPage.getFirstWorkflowCase();
    expect(findWorkflowCase.id).equal(1);
    expect(findWorkflowCase.status, 'status not updated').equal('Ongoing');
  });
});
