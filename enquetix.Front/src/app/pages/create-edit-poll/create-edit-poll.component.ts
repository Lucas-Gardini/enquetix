import { Component, OnDestroy } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { Card } from "primeng/card";
import { Subscription } from "rxjs";
import { LogoComponent } from "../../components/logo/logo.component";
import { PollFormComponent } from "../../components/poll-form/poll-form.component";
import { IPollDetails } from "../poll/poll.component";
import { PollService } from "../poll/poll.service";
import { MessageService } from "primeng/api";
import { ProgressSpinnerModule } from "primeng/progressspinner";

import { trigger, transition, style, animate } from "@angular/animations";

@Component({
  selector: "app-create-edit-poll",
  imports: [LogoComponent, Card, PollFormComponent, ProgressSpinnerModule],
  templateUrl: "./create-edit-poll.component.html",
  styleUrl: "./create-edit-poll.component.scss",
  animations: [
    trigger("fadeInOut", [
      transition(":enter", [
        style({ opacity: 0 }),
        animate("300ms ease-in", style({ opacity: 1 })),
      ]),
      transition(":leave", [animate("300ms ease-out", style({ opacity: 0 }))]),
    ]),
  ],
})
export class CreatePollComponent implements OnDestroy {
  subscription: Subscription | null = null;
  pollId: string | null = null;

  public pollData: IPollDetails | null = null;
  public createdByLoggedUser = false;
  public loading = true;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private pollService: PollService,
    private messageService: MessageService
  ) {
    this.subscription = this.route.queryParamMap.subscribe((params) => {
      this.pollId = params.get("id") ?? null;
      if (this.pollId) {
        this.pollService
          .loadPollDetails(this.pollId)
          .then(async (pData) => {
            this.pollData = pData;
            this.createdByLoggedUser = await this.pollService.validateAccess(
              this.pollData?.pollDetails!,
              this.pollId!
            );

            if (!this.createdByLoggedUser) {
              this.messageService.add({
                severity: "error",
                summary: "Acesso Negado",
                detail: "Você não tem permissão para editar esta enquete.",
              });
              this.pollId = null;
              this.router.navigate(["/"]);
            }
          })
          .finally(() => {
            this.loading = false;
          });
      } else {
        this.loading = false;
      }
    });
  }

  ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
