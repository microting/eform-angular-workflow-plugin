/*
The MIT License (MIT)
Copyright (c) 2007 - 2021 Microting A/S
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections.Generic;

namespace Workflow.Pn.Infrastructure.Models.Cases
{
    using System;

    public class WorkflowCasesUpdateModel
    {
        public WorkflowCasesUpdateModel()
        {
            PicturesOfTask = new List<FieldValue>();
            PicturesOfTaskDone = new List<FieldValue>();
        }

        public int Id { get; set; }

        public string DateOfIncident { get; set; }

        public string CreatedBySiteName { get; set; }

        public string IncidentPlace { get; set; }

        public int? IncidentPlaceId { get; set; }

        public string IncidentPlaceListId { get; set; }

        public string IncidentType { get; set; }

        public int? IncidentTypeId { get; set; }

        public string IncidentTypeListId { get; set; }

        public string Description { get; set; }

        public string Deadline { get; set; }

        public string ActionPlan { get; set; }

        public int? ToBeSolvedById { get; set; }

        public int? Status { get; set; }

        public List<FieldValue> PicturesOfTask { get; set; } = new List<FieldValue>();

        public List<FieldValue> PicturesOfTaskDone { get; set; } = new List<FieldValue>();
    }
}
