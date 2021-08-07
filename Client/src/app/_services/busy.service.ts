import { Injectable } from '@angular/core';
import { NgxUiLoaderService } from 'ngx-ui-loader';

@Injectable({
  providedIn: 'root'
})
export class BusyService {
  busyRequstCount = 0;

  constructor(private spinnerService: NgxUiLoaderService) { }

  busy(){
    this.busyRequstCount++;
    this.spinnerService.start();
  }

  idle(){
    this.busyRequstCount--;
    if(this.busyRequstCount <=0) {
      this.busyRequstCount = 0;
      this.spinnerService.stop();
    }
  }
}
