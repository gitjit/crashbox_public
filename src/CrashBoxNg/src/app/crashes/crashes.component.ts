import { Component, OnInit } from "@angular/core";
import { ICrash } from "../models/crash";
import { TopCrash } from "../models/topcrash";
import { CrashService } from "../services/crash.service";
import { ActivatedRoute } from "@angular/router";
import * as moment from 'moment';

@Component({
  selector: "app-crashes",
  templateUrl: "./crashes.component.html",
  styleUrls: ["./crashes.component.css"],
})
export class CrashesComponent implements OnInit {
  pageTitle: string = "Crashes";
  filters: Array<string> = ["All", "Top 10"];
  pk: string;
  filter: string;
  page: number = 0;

  crashes: ICrash[] = [];
  topCrashes: TopCrash[] = [];
  progress: boolean;

  constructor(
    private crashService: CrashService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.pk = this.route.snapshot.paramMap.get("pk");
    this.progress = true;
    this.loadCrashes();
    //this.loadAppVersions();
  }

  getTitle(): string {
    return this.pageTitle;
  }

  filterChanged(filter: string) {
    if (this.filter != filter){
      this.filter = filter;
      console.log(this.filter);
    }
  }

  getDate(dt) {
    return moment(dt * 1000).fromNow();
  }

  onReload($event) {
    $event.preventDefault();
    if (this.filter == "All" && this.crashes.length == 0) {
      this.loadCrashes();
      this.topCrashes = [];
    } else if(this.filter == 'Top 10' && this.topCrashes.length == 0){
      this.loadTopCrashes();
      this.crashes = [];
    }
  }

  loadTopCrashes() {
    this.progress = true;
    this.crashService.getTop10(this.pk).subscribe({
      next: (crashes) => {
        this.topCrashes = crashes;
        console.log("top count = " + this.topCrashes[0].count);
        this.progress = false;
      },
      error: (err) => {
        this.progress = false;
        console.log(err);
      },
    });
  }

  loadCrashes() {
    this.progress = true;
    this.crashService.getCrashes(this.pk, this.page).subscribe({
      next: (crashes) => {
        this.crashes = crashes;
        this.progress = false;
      },
      error: (err) => {
        this.progress = false;
        console.log(err);
      },
    });
  }

  onBack(): void {
    if (this.page >= 1) this.page--;
    this.loadCrashes();
  }

  onNext(): void {
    this.page++;
    this.loadCrashes();
  }


}
