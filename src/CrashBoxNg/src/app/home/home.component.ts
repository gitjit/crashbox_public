import { Component, OnInit } from "@angular/core";
import { CrashService } from "../services/crash.service";
import { Project } from "../models/project";
import * as moment from "moment"; // add this 1 of 4
import { environment } from "src/environments/environment";
import { MsalService } from "@azure/msal-angular";

@Component({
  selector: "app-home",
  templateUrl: "./home.component.html",
  styleUrls: ["./home.component.css"],
})
export class HomeComponent implements OnInit {
  progress: boolean = false;
  projects: Project[] = [];
  showWelcome: boolean = true;
  btnName: string;
  organization: string;

  subTitle1Text: string = "You are not signed In";
  subTitle2Text: string =
    "Please log-in with your organization account to proceed";

  subTitle1LoggedInText: string = `Organanization selected is `;
  subTitle2LoggedInText: string = "Click 'Load Projects' to dowload crash details";

  subTitle1: string = "You are not signed In";
  subTitle2: string = "Please log-in with your organization account to proceed";

  constructor(
    private crashService: CrashService,
    private authService: MsalService
  ) {}

  async ngOnInit() {
    var loggedIn = await this.crashService.isAuthenticated();
    if (loggedIn) {
      this.btnName = "Load Projects";
      this.organization = this.crashService.getOrganization();
      this.subTitle1 = this.subTitle1LoggedInText + this.organization;
      this.subTitle2 = this.subTitle2LoggedInText;
      
    } else {
      this.btnName = "Login";
      this.subTitle1 = this.subTitle1Text;
      this.subTitle2 = this.subTitle2Text;
      this.organization = "";
     
    }
    console.log("Is Production = " + environment.production);
  }

  onLogin() {
    if(this.btnName == "Load Projects")
    {
        this.showWelcome = false;
        //this.crashService.getWeather().subscribe(result => console.log(result));
        this.loadProjects();
    }
    else{
      this.crashService.loginStatus$.subscribe((loginStatus) => {
        console.log("Login status updated: " + loginStatus);
        //this.isLoggedIn = loginStatus;
        if (loginStatus) {
          this.btnName = "Load Projects";
          this.organization = this.crashService.getOrganization();
          this.subTitle1 = this.subTitle1LoggedInText + this.organization;
          this.subTitle2 = this.subTitle2LoggedInText;
        }
      });
      this.crashService.logIn();
    }
   
  }

  loadProjects() {
    this.progress = true;
    this.crashService.getProjects().subscribe({
      next: (projects) => {
        for (var proj of projects) {
          let ap = new Project();
          ap.name = proj;
          this.projects.push(ap);
          console.log("inside load projects" + ap.name);
          this.crashService.getLastCrash(ap.name).subscribe({
            next: (cr) => {
              console.log("inside load last crash " + cr._ts + cr.version);
              ap.lastEntry = cr._ts;
              this.crashService.getCrashCount(ap.name).subscribe({
                next: (cnt) => {
                  ap.count = cnt;
                  //this.projects.push(ap);
                },
                error: (err) => console.log(err),
              });
            },
            error: (err) => console.log(err),
          });
        }
        this.progress = false;
      },
      error: (err) => {
        console.log(err);
      },
    });
  }

  getDate(dt) {
    return moment(dt * 1000).fromNow();
  }
}
