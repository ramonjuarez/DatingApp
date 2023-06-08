import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { AccountService } from '../_services/account.service';
import { ToastrService } from 'ngx-toastr';
import { AbstractControl, FormBuilder, FormControl, FormGroup, ValidatorFn, Validators } from '@angular/forms';
import { Router } from '@angular/router';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit{
  @Output() cancelRegister = new EventEmitter();
  registerForm : FormGroup = new FormGroup({});
  maxDate: Date = new Date();
  validationErrors : string[] | undefined;

  constructor (private accountService: AccountService, private toastr: ToastrService,
    private formBuilder: FormBuilder, private router: Router){
  }

  ngOnInit(): void {
    this.initializeForm();
    this.maxDate.setFullYear(this.maxDate.getFullYear() - 18);
  }

  register(){
    const dob = this.getDateOny(this.registerForm.controls['dateOfBirth'].value);
    const values = {...this.registerForm.value, dateOfBirth: dob};
    //console.log(values);
    this.accountService.register(values).subscribe({
      next: () => {
        this.router.navigateByUrl('/members');
      },
      error : error=> {
        this.validationErrors = error;
      }
    })
  }

  cancel(){
    this.cancelRegister.emit(false);
  }

  initializeForm(){
    this.registerForm = this.formBuilder.group({
      username: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.matchValues('password')]],
      gender: ['male'],
      knownAs: ['', Validators.required],
      dateOfBirth: ['', Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
    });
    this.registerForm.controls['password'].valueChanges.subscribe({
      next: _ => this.registerForm.controls['confirmPassword'].updateValueAndValidity()
    })
  }

  matchValues(matchTo: string): ValidatorFn{
    return (control: AbstractControl) => {
      return control.value === control.parent?.get(matchTo)?.value ? null : {notMatching: true};
    }
  }

  private getDateOny(dob: string | undefined){
    if(!dob) return;

    let dateOfBirth = new Date(dob);
    return new Date(
        dateOfBirth.setMinutes(dateOfBirth.getMinutes() - dateOfBirth.getTimezoneOffset())
      ).toISOString().slice(0,10);

  }

}
