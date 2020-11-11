import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { CrashesComponent } from './crashes/crashes.component';
import { CrashDetailComponent } from './crashes/crash-detail.component';
import { AccountComponent } from './account/account.component';

const routes: Routes = [
  {
    path:'home', component: HomeComponent
  },
  {
    path:'', redirectTo: 'home',pathMatch:'full'
  },
  {
    path:'account', component: AccountComponent
  },
  {
    path:'crashes/:pk', component: CrashesComponent
  },
  {
    path:'crashes/:pk/:id', component: CrashDetailComponent
  },
  {
    path:'**', redirectTo: 'home',pathMatch:'full'
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
