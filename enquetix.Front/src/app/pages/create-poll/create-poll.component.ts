import { Component } from "@angular/core";
import { LogoComponent } from "../../components/logo/logo.component";
import { Card } from "primeng/card";
import { PollFormComponent } from "../../components/poll-form/poll-form.component";

@Component({
  selector: "app-create-poll",
  imports: [LogoComponent, Card, PollFormComponent],
  templateUrl: "./create-poll.component.html",
  styleUrl: "./create-poll.component.scss",
})
export class CreatePollComponent {}
