import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';

import {HttpClientModule} from '@angular/common/http';
import {MsalModule} from '@azure/msal-angular'

@NgModule({
  declarations: [
    AppComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    MsalModule.forRoot({
      auth:{
        clientId:"44d08264-ac20-4d86-96e7-c85e7d5c5e4a",
        authority:"https://crashbox.b2clogin.com/crashbox.onmicrosoft.com/B2C_1_sisu",
        validateAuthority:false,
        redirectUri:"http://localhost:4200/"
      },
      cache:{
        cacheLocation:"sessionStorage",
        storeAuthStateInCookie:false
      }
    },{
      consentScopes:[
        "user.read","openid","profile"
      ]
    })
    
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
