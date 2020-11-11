import { Component, OnInit } from "@angular/core";
import { MsalService, BroadcastService } from "@azure/msal-angular";
import { Subscription } from "rxjs";
import { HttpClient } from "@angular/common/http";
import { b2cPolicies } from "../app-config";
import { CrashService } from '../services/crash.service';

@Component({
  selector: "app-navbar",
  templateUrl: "./navbar.component.html",
  styleUrls: ["./navbar.component.css"],
})
export class NavbarComponent implements OnInit {
  accountName = "";
  isLoggedIn = false;
  subscriptions: Subscription[] = [];

  constructor(
    private authService: MsalService,
    private crashService: CrashService,
    private broadcastService: BroadcastService,
    private http: HttpClient
  ) {}

  ngOnInit(){
    this.checkAccount();
    this.crashService.loginStatus$.subscribe((loginStatus) => {
      console.log('Login status updated: '+ loginStatus);
      this.isLoggedIn = loginStatus;
      this.checkAccount();
    });
    
  }

  onLogin() {
    if (this.isLoggedIn) {
      this.crashService.logOut();
    } else {
      this.crashService.logIn();
    }
  }

  async checkAccount() {
    this.isLoggedIn = !!this.authService.getAccount();
    if (this.isLoggedIn) {
      this.accountName = this.authService.getAccount().name;
    } else {
      this.accountName = "";
    }
  }
}
