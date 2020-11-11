import { Component, OnInit } from "@angular/core";
import { MsalService } from "@azure/msal-angular";

@Component({
  selector: "app-account",
  templateUrl: "./account.component.html",
  styleUrls: ["./account.component.css"],
})
export class AccountComponent implements OnInit {
  accountName: string;
  progress = false;
  org: string;
  apiKey: string;

  constructor(private authService: MsalService) {}

  ngOnInit(): void {
    let account = this.authService.getAccount();
    console.log(account);
    this.accountName = account.name;
    this.org = account.idToken.extension_Organization;
    this.apiKey = "12334353433";
  }
}
