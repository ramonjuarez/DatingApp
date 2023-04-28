import { Injectable } from '@angular/core';
import {
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpInterceptor,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, catchError } from 'rxjs';
import { NavigationExtras, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private router: Router, private toastr: ToastrService) { }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {

    return next.handle(request).pipe(
      catchError((httpError: HttpErrorResponse) => {
        
        if (httpError) {
          switch (httpError.status) {
            case 400:
              if (this.hasListOfErrors(httpError))
                throw this.getListOfErrors(httpError);
              else {
                this.fireToastrErrorMessage(httpError.error, httpError.status.toString());
              }
              break;
            case 401:
              this.fireToastrErrorMessage("Unauthorized", httpError.status.toString());
              break;
            case 404:
              this.router.navigateByUrl("/not-found");
              break;
            case 500:
              const navigationExtras: NavigationExtras = { state: { error: httpError.error } };
              this.router.navigateByUrl("/server-error",navigationExtras);
              console.log(httpError);
              break;
            default:
              this.fireToastrErrorMessage("Something unexpected went wrong", "");
              console.log(httpError);
          }
        }
        throw httpError;
      })
    )
  }

  fireToastrErrorMessage(errorMessage: string, errorCode: string) {
    this.toastr.error(errorMessage, errorCode);
  }

  hasListOfErrors(httpError: HttpErrorResponse) {
    return httpError.error.errors;
  }

  getListOfErrors(httpError: HttpErrorResponse) {
    const modelStateErrors = [];
    for (const key in httpError.error.errors) {
      if (httpError.error.errors[key]) {
        modelStateErrors.push(httpError.error.errors[key]);
      }
    }
    return modelStateErrors;
  }

}
