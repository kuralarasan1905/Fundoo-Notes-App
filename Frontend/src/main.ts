import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { importProvidersFrom } from '@angular/core';
import { MaterialModule } from './app/shared/material.module';

// Enhanced app config with Material modules
const enhancedAppConfig = {
  ...appConfig,
  providers: [
    ...appConfig.providers,
    importProvidersFrom(MaterialModule)
  ]
};

bootstrapApplication(AppComponent, enhancedAppConfig)
  .catch((err) => console.error(err));
