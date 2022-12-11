import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, NgForm, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { SharedService } from 'src/app/shared.service';
import { EmployeeDto } from '../SharedDTO/employee.dto';
import { NavMenuComponent } from '../nav-menu/nav-menu.component';


@Component({
  selector: 'app-log-in',
  templateUrl: './log-in.component.html',
  styleUrls: ['./log-in.component.css']
})
export class LogInComponent implements OnInit {

  public static SignedInEmployee: EmployeeDto | undefined = {
      employeeId: "",
      firstName: "",
      lastName: "",
      email: "",
      role: "",
      password: "",
      isAdmin: false,
      isSuperAdmin: false
    };
  employeeId: string = "";
  employeePassword: string = "";
  public loginForm!: FormGroup;

  constructor(public service: SharedService, private formBuilder: FormBuilder, private router: Router) { }

  ngOnInit(): void {
    this.loginForm = this.formBuilder.group({
      employeeId: ["", Validators.required],
      employeePassword:["", Validators.required]
    })
  }

  LogInUser(form: NgForm) {
    this.GetEmployee(form.controls.employeeId.value);
    if (LogInComponent.SignedInEmployee?.employeeId == this.employeeId && LogInComponent.SignedInEmployee?.password == this.employeePassword) {
      if (LogInComponent.SignedInEmployee.isAdmin == true) {
        this.router.navigate(['/admin-dashboard']);
      } else {
        this.router.navigate(['/dashboard']);
      }
    } else {
      this.loginForm.reset();
    }
  }

  GetEmployee(employeeId: any) {
    return this.service.GetEmployee(employeeId).subscribe((data: EmployeeDto) => {
      if (LogInComponent.SignedInEmployee != undefined) {
        LogInComponent.SignedInEmployee.employeeId = data.employeeId;
        LogInComponent.SignedInEmployee.firstName = data.firstName;
        LogInComponent.SignedInEmployee.lastName = data.lastName;
        LogInComponent.SignedInEmployee.role = data.role;
        LogInComponent.SignedInEmployee.email = data.email;
        LogInComponent.SignedInEmployee.password = data.password;
        LogInComponent.SignedInEmployee.isAdmin = data.isAdmin;
        LogInComponent.SignedInEmployee.isSuperAdmin = data.isSuperAdmin;

        NavMenuComponent.showAdministrativeSettings = data.isAdmin;
      }
    });
  }
}
