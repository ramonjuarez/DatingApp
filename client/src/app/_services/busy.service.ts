import { Injectable } from '@angular/core';
import { NgxSpinnerService } from 'ngx-spinner';

@Injectable({
  providedIn: 'root'
})
export class BusyService {
  busyRequestCount = 0 ;

  constructor(private spinner: NgxSpinnerService) { }

  busy(){
    this.busyRequestCount ++;
    this.spinner.show(undefined, {
      type: 'pacman',
      bdColor: 'rgba(255,255,255,0)',
      color: '#33333',
    })
  }

  idle(){
    this.busyRequestCount--;
    if(this.busyRequestCount <= 0){
      this.busyRequestCount = 0;
      this.spinner.hide();
    }
  }

}
