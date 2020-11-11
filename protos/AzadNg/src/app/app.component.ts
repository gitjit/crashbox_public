import { Component } from '@angular/core';
import { MsalService } from '@azure/msal-angular';

@Component({
  selector: 'app-root',
  templateUrl:'app.component.html',
  styles: []
})
export class AppComponent {
  title = 'AzadNg';
  btnTitle = "Log in";
  loggedIn = false;
  userName = "";

  constructor(private authService:MsalService){
    this.checkAccount();
  }

  onLogin(){
    if(this.loggedIn){
      this.authService.logout();
    }
    else{
      console.log('OnLogin');
      this.authService.loginPopup().then(result => {
        console.log("Login result = ",result);
      }).catch(err => console.log("Login Error:",err))
    }
    this.checkAccount();
  } 

  checkAccount(){
    console.log('checkAccount')
    this.loggedIn = !!this.authService.getAccount();
    console.log('this.loggedIn = ',this.loggedIn);
    if(this.loggedIn){
      this.btnTitle = "Log Out";
      this.userName = this.authService.getAccount().name;
    }
    else{
      this.btnTitle = "Log In"
    }
  }
}
