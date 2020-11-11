import { Injectable, Inject, OnInit } from "@angular/core";
import {
  HttpClient,
  HttpErrorResponse,
  HttpHeaders,
} from "@angular/common/http";
import { Observable, throwError, pipe, Subscription, Subject } from "rxjs";

import { catchError, tap, map } from "rxjs/operators";
import { ICrash } from "../models/crash";
import { TopCrash } from "../models/topcrash";
import { MsalService, BroadcastService } from "@azure/msal-angular";
import { b2cPolicies } from "../app-config";
import { AuthResponse } from "msal";

@Injectable({
  providedIn: "root",
})
export class CrashService {
  //private crashUrl = "https://crashbox.azurewebsites.net/api/crash"; // Function App
  //private crashUrl = "https://crashbox.azure-api.net/api/crash"; // API gateway
  //private crashUrl = "https://localhost:5001/api/crash"; // Local web api
  private crashUrl = "https://crashboxweb.azurewebsites.net/api/crash"; // App service web api
  //private crashUrl = "http://localhost:7071/api/crash"; // Az Fun local
  private accessToken: any;
  private subscriptions: Subscription[] = [];
  private authHeader: any;
  private isLoggedIn: boolean = false;

  private _loginStatusSource = new Subject<boolean>();
  loginStatus$ = this._loginStatusSource.asObservable();

  constructor(
    private http: HttpClient,
    private authService: MsalService,
    private broadcastService: BroadcastService
  ) {
    //this.getAccessToken();
    this.init();
  }

  init(): void {
    let loginSuccessSubscription: Subscription;
    let loginFailureSubscription: Subscription;

    loginSuccessSubscription = this.broadcastService.subscribe(
      "msal:loginSuccess",
      async (success) => {
        // We need to reject id tokens that were not issued with the default sign-in policy.
        // "acr" claim in the token tells us what policy is used (NOTE: for new policies (v2.0), use "tfp" instead of "acr")
        // To learn more about b2c tokens, visit https://docs.microsoft.com/en-us/azure/active-directory-b2c/tokens-overview
        if (success.idToken.claims["acr"] === "B2C_1_signupin") {
          window.alert(
            "Password has been reset successfully. \nPlease sign-in with your new password"
          );
          return this.authService.logout();
          this._loginStatusSource.next(false);
        }
        console.log(
          "login succeeded. id token acquired at: " + new Date().toString()
        );
        await this.getAccessToken();
        this._loginStatusSource.next(true);
        console.log(success);
      }
    );

    loginFailureSubscription = this.broadcastService.subscribe(
      "msal:loginFailure",
      (error) => {
        console.log("login failed");
        console.log(error);
        this._loginStatusSource.next(false);
        this.accessToken = "";
        // Check for forgot password error
        // Learn more about AAD error codes at https://docs.microsoft.com/en-us/azure/active-directory/develop/reference-aadsts-error-codes
        if (error.errorMessage.indexOf("AADB2C90118") > -1) {
          {
            this.authService.loginPopup(b2cPolicies.authorities.resetPassword);
          }
        }
      }
    );

    this.subscriptions.push(loginSuccessSubscription);
    this.subscriptions.push(loginFailureSubscription);
  }

  private handleError(err: HttpErrorResponse) {
    console.log("Inside handle error" + err);
    let errorMessage = "";
    if (err.error instanceof ErrorEvent) {
      errorMessage = `An error occured ${err.error.message}`;
    } else {
      errorMessage = `Server returned code : ${err.status}, error message is : ${err.message}`;
    }
    console.error(errorMessage);
    return throwError(errorMessage);
  }

  // GET CRASHES
  getCrashes(pk: string, page: number): Observable<ICrash[]> {
    let api = this.crashUrl + "?" + "page=" + page + "&pk=" + pk;
    console.log("GetCrashes : " + api);
    const header = {
      Authorization: `Bearer ${this.accessToken}`,
    };
    return this.http
      .get<ICrash[]>(api, { headers: header })
      .pipe(
        tap((data) => {
          for (let cr of data) {
            console.log(cr._ts);
          }
        }),
        catchError(this.handleError)
      );
  }

  // GET Projects
  getProjects(): Observable<string[]> {
    let api = this.crashUrl + "?" + "qp=projects";
    const header = {
      Authorization: `Bearer ${this.accessToken}`,
    };
    console.log("GetCrashes : " + api);
    return this.http
      .get<string[]>(api, { headers: header })
      .pipe(
        tap((data) => {
          for (let item of data) {
            console.log(item);
          }
        }),
        catchError(this.handleError)
      );
  }

  // Get LAST CRASH IN APP
  getLastCrash(pk: string): Observable<ICrash> {
    let api = this.crashUrl + "?" + "qp=latest&pk=" + pk;
    const header = {
      Authorization: `Bearer ${this.accessToken}`,
    };
    console.log("GetLastCrash : " + api);
    return this.http
      .get<ICrash>(api, { headers: header })
      .pipe(
        tap((data) => {
          console.log("getLastCrash : Response = " + data._ts);
        }),
        catchError(this.handleError)
      );
  }

  // Get CRASH COUNT
  getCrashCount(pk: string): Observable<number> {
    let api = this.crashUrl + "?" + "qp=count&pk=" + pk;
    const header = {
      Authorization: `Bearer ${this.accessToken}`,
    };
    console.log("GetCrashCount : " + api);
    return this.http
      .get<number>(api, { headers: header })
      .pipe(
        tap((data) => {
          console.log("GetCrashCount : Response = " + data);
        }),
        catchError(this.handleError)
      );
  }

  // GET CRASH DETAILS
  getCrashDetails(pk: string, id: string) {
    let api = this.crashUrl + "?pk=" + pk + "&id=" + id;
    console.log("GetCrashDetails : " + api);
    const header = {
      Authorization: `Bearer ${this.accessToken}`,
    };
    return this.http
      .get<ICrash>(api, { headers: header })
      .pipe(
        tap((data) => {
          console.log("GetCrashDetails : Response = " + data);
        }),
        catchError(this.handleError)
      );
  }

  // GET TOP 10 CRASHES
  getTop10(pk: string): Observable<TopCrash[]> {
    let api = this.crashUrl + "?qp=top10&pk=" + pk;
    console.log("GetTop10Crashes : " + api);
    const header = {
      Authorization: `Bearer ${this.accessToken}`,
    };
    return this.http
      .get<TopCrash[]>(api, { headers: header })
      .pipe(
        tap((data) => {
          console.log("GetTopCrash : Response = " + data);
        }),
        catchError(this.handleError)
      );
  }

  // Login User
  logIn() {
    if (this.isLoggedIn) {
      this.authService.logout();
    } else {
      this.authService
        .loginPopup()
        .then((result) => {
          console.log("Login success", result);
          this.isLoggedIn = true;
        })
        .catch((err) => {
          console.log("Login failed : ", err);
        });
    }
  }

  //Log out
  logOut() {
    this.authService.logout();
    this.accessToken = "";
    this._loginStatusSource.next(false);
  }

  //Grab the access token
  async getAccessToken(): Promise<string> {
    console.log("CrashService:" + "getAccessToken()");
    var result = await this.authService.acquireTokenSilent({
      scopes: ["https://crashbox.onmicrosoft.com/api/read_crash"],
    });
    this.accessToken = result.accessToken;
    console.log(result.accessToken);
    return result.accessToken;
  }

  // Grab the user name
  getUserName(): string {
    let account = this.authService.getAccount();
    if (account) {
      return account.name;
    } else {
      return "";
    }
  }

  // Grab the organization name
  getOrganization(): string {
    let account = this.authService.getAccount();
    if (account) {
      return account.idToken.extension_Organization;
    } else {
      return "";
    }
  }

  // Is user authenticated or not
  async isAuthenticated(): Promise<boolean> {
    this.isLoggedIn = !!this.authService.getAccount();
    if (this.isLoggedIn) {
      await this.getAccessToken();
      if (this.accessToken) {
        return true;
      } else {
        return false;
      }
    }
    return this.isLoggedIn;
  }

  // gCrashes() {
  //   let api = "https://crashbox.azure-api.net/api/crash?page=1&pk=CBox_1.0";
  //   let token = `eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6Ilg1ZVhrNHh5b2pORnVtMWtsMll0djhkbE5QNC1jNTdkTzZRR1RWQndhTmsifQ.eyJpc3MiOiJodHRwczovL2NyYXNoYm94LmIyY2xvZ2luLmNvbS8xODA1NjRjMi0yM2MzLTQ4NmYtYTg4Ni05OTJjYjM1YjFjYjkvdjIuMC8iLCJleHAiOjE2MDM2OTI0NzMsIm5iZiI6MTYwMzY4ODg3MywiYXVkIjoiNTgxZmZjMGQtNTU5Yy00ZjcwLWJiMDQtN2ZjNmIyZjZkYmVlIiwic3ViIjoiNWNkMmMwZWUtNGNhNi00YjExLTg4YTItMGFjMWQ3MmRlMmRkIiwibmFtZSI6IkppdGhlc2giLCJleHRlbnNpb25fT3JnYW5pemF0aW9uIjoiTWFkaXlhbiIsInRmcCI6IkIyQ18xX3Npc3UiLCJub25jZSI6Ijk1MTk3Yzk2LTRmZjAtNDhhNi05MmYxLWE3ZTg4ZDE4YzA1NiIsInNjcCI6InJlYWRfY3Jhc2giLCJhenAiOiJkMTZjNGYzOC1lNTdmLTRlMjEtOGQxNi04OTlkYjAxNjRiYTciLCJ2ZXIiOiIxLjAiLCJpYXQiOjE2MDM2ODg4NzN9.h76avVofAfPaHgcVM_7vE4zNrGB_2odcrHJewWt-IXIvY2tXKkboUxg2e5FEQ-staQ1sHrzYZYDvH3veGeMqZxjkSTfZ9uFW4CwPWgjn3BcTPBN86nP9tkBT6pReoHS1tcrPlcS6SiEcwciyk-zY0jyhAQTDBOVFu_SJS8joi40Bib_kE7gRCPZ6jVI5uMVu1ltbH0bhb79LyKhHCyARbcydUftrAVtZ3uCOSO42mPzSzCx3jKoa-13JeLgsKhvBmdMV-lEiGu5UWVbXRipQYJvuQh6WvRG6W3ENa6r5EE6wBg1d4Ws4-fi4X1Ch3d0X6zuxZuHYCmi2_5ABGN1xWg`;
  //   const header = {
  //     'Authorization': `Bearer ${token}`
  //   };
  //   return this.http.get(api, {headers:header}).pipe(catchError(this.handleError));
  //   //return this.http.get(api, {headers:header}).pipe(map((res) => console.log(JSON.stringify(res))));
  // }
}
