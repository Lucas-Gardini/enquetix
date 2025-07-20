import { Component } from "@angular/core";
import { CardModule } from "primeng/card";
import { InputTextModule } from "primeng/inputtext";
import { ButtonModule } from "primeng/button";
import { DividerModule } from "primeng/divider";
import { SelectButtonModule } from "primeng/selectbutton";
import { FormsModule } from "@angular/forms";
import { LoginFormComponent } from "../../components/login-form/login-form.component";
import { Router } from "@angular/router";
import { LogoComponent } from "../../components/logo/logo.component";

@Component({
  selector: "app-index",
  imports: [
    CardModule,
    InputTextModule,
    ButtonModule,
    DividerModule,
    SelectButtonModule,
    FormsModule,
    LoginFormComponent,
    LogoComponent,
  ],
  templateUrl: "./index.component.html",
  styleUrl: "./index.component.scss",
})
export class IndexComponent {
  stateOptions: { label: string; value: string }[] = [
    { label: "Enquetes", value: "polls" },
    { label: "Minhas Enquetes", value: "my-polls" },
  ];
  value: string = "polls";

  constructor(private router: Router) {}

  public newPoll() {
    this.router.navigate(["/create-poll"]);
  }
}
