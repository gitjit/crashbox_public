import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";

import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { HomeComponent } from "./home/home.component";
import { NavbarComponent } from "./navbar/navbar.component";
import { HttpClientModule, HTTP_INTERCEPTORS } from "@angular/common/http";
import { CrashesComponent } from "./crashes/crashes.component";
import { CrashFormatPipe } from "./pipes/CrashFormatPipe";
import { CrashDetailComponent } from "./crashes/crash-detail.component";
import { MsalModule, MSAL_CONFIG, MsalService, MSAL_CONFIG_ANGULAR, MsalAngularConfiguration, MsalInterceptor } from "@azure/msal-angular";
import { Configuration } from "msal";
import { msalConfig, msalAngularConfig, msalConfigDev } from "./app-config";
import { AccountComponent } from './account/account.component';
import { environment } from 'src/environments/environment';

function MSALConfigFactory(): Configuration {
  if(environment.production)
  {
    console.log('prod config returned');
    return msalConfig;
  }
  else
  {
    console.log('dev config returned');
    return msalConfigDev;
  }
}
function MSALAngularConfigFactory(): MsalAngularConfiguration {
  return msalAngularConfig;
}

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    NavbarComponent,
    CrashesComponent,
    CrashFormatPipe,
    CrashDetailComponent,
    AccountComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    MsalModule,
    // MsalModule.forRoot(
    //   {
    //     auth: {
    //       clientId: "d16c4f38-e57f-4e21-8d16-899db0164ba7", // Application Id of Application registered in B2C
    //       authority:
    //         "https://crashbox.b2clogin.com/crashbox.onmicrosoft.com/B2C_1_sisu", //signup-signin userflow
    //       validateAuthority: false,
    //       //redirectUri: "http://localhost:4200/",
    //       redirectUri: "https://crashbox.z5.web.core.windows.net/",
    //     },
    //     cache: {
    //       cacheLocation: "sessionStorage",
    //       storeAuthStateInCookie: false,
    //     },
    //   },
    //   {
    //     consentScopes: ["user.read", "openid", "profile"],
    //   }
    // ),
  ],
  providers: [
    {
      provide: HTTP_INTERCEPTORS,
      useClass: MsalInterceptor,
      multi: true
    },
    {
      provide: MSAL_CONFIG,
      useFactory: MSALConfigFactory,
    },
    {
      provide: MSAL_CONFIG_ANGULAR,
      useFactory: MSALAngularConfigFactory
    },
    MsalService
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
