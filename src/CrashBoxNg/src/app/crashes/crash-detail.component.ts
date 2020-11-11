import { Component, OnInit } from '@angular/core';
import { CrashService } from '../services/crash.service';
import { ActivatedRoute, Router } from '@angular/router';
import { ICrash } from '../models/crash';

@Component({
  selector: 'app-crash-detail',
  templateUrl: './crash-detail.component.html',
  styleUrls: ['./crash-detail.component.css']
})
export class CrashDetailComponent implements OnInit {

  pageTitle:string = "Crash Details";
  pk:string;
  id:string;
  crash:ICrash;
  progress:boolean;

  constructor(private crashService:CrashService,private route: ActivatedRoute,private router: Router) { }

  ngOnInit(): void {
    this.progress = true;
    this.pk = this.route.snapshot.paramMap.get('pk');
    this.id = this.route.snapshot.paramMap.get('id');
    console.log(this.pk + this.id);
    this.loadCrashDetails();
  }

  loadCrashDetails():void{
    this.crashService.getCrashDetails(this.pk,this.id).subscribe({
      next:(cr)=>{
          console.log(cr.id);
          this.crash = cr;
          this.progress = false;
      },
      error:(err) => console.log(err)
    })
  }

  onBack():void{
    console.log('onback');
    let bk = '/crashes/'+this.pk;
    this.router.navigate([bk]);
  }

}
