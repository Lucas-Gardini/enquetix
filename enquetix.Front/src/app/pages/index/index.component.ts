import { Component, OnInit } from "@angular/core";
import { CardModule } from "primeng/card";
import { InputTextModule } from "primeng/inputtext";
import { ButtonModule } from "primeng/button";
import { DividerModule } from "primeng/divider";
import { SelectButtonModule } from "primeng/selectbutton";
import { FormsModule } from "@angular/forms";
import { LoginFormComponent } from "../../components/login-form/login-form.component";
import { Router } from "@angular/router";
import { LogoComponent } from "../../components/logo/logo.component";
import { PollsComponent } from "../../components/polls/polls.component";
import { ApiService } from "../../services/api.service";

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
    PollsComponent,
  ],
  templateUrl: "./index.component.html",
  styleUrl: "./index.component.scss",
})
export class IndexComponent implements OnInit {
  stateOptions: { label: string; value: string }[] = [
    { label: "Minhas Enquetes", value: "polls" },
    { label: "Minha Conta", value: "my-account" },
  ];
  value: string = "polls";

  loggedIn: boolean = false;
  searchTerm: string = "";

  constructor(private api: ApiService, private router: Router) {}

  async ngOnInit() {
    try {
      const isLogged = await this.api.getUserProfile();
      if (!isLogged) throw new Error("User not logged in");

      this.loggedIn = true;
    } catch (error) {
      console.error("Error checking login status:", error);
      this.value = "my-account";
    }
  }

  public newPoll() {
    this.router.navigate(["/create-poll"]);
  }

  public setSearchTerm(term: string) {
    this.searchTerm = term;
    this.router.navigate([], {
      queryParams: { search: term },
      queryParamsHandling: "merge",
    });
  }
}
