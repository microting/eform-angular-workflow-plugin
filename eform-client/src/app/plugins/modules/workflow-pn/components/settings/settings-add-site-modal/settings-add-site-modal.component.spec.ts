import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { provideNoopAnimations } from '@angular/platform-browser/animations';
import { TranslateModule } from '@ngx-translate/core';
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { WorkflowPnSettingsService } from '../../../services';
import { SettingsAddSiteModalComponent } from './settings-add-site-modal.component';

describe('SettingsAddSiteModalComponent', () => {
  let component: SettingsAddSiteModalComponent;
  let fixture: ComponentFixture<SettingsAddSiteModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SettingsAddSiteModalComponent, TranslateModule.forRoot()],
      providers: [
        provideNoopAnimations(),
        { provide: WorkflowPnSettingsService, useValue: { addSiteToSettings: vi.fn() } },
        { provide: MatDialogRef, useValue: { close: vi.fn() } },
        {
          provide: MAT_DIALOG_DATA,
          useValue: {
            sites: [{ siteUId: 1, siteName: 'A' }, { siteUId: 2, siteName: 'B' }],
            assignedSites: [{ siteUId: 1, siteName: 'A' }],
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SettingsAddSiteModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should only offer sites that are not assigned yet', () => {
    expect(component.availableSites.map((s) => s.siteUId)).toEqual([2]);
  });
});
