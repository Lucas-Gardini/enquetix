import { Component, OnDestroy, OnInit } from "@angular/core";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { ApiService } from "../../services/api.service";
import { Subscription } from "rxjs";
import { LogoComponent } from "../../components/logo/logo.component";
import { Card } from "primeng/card";
import { Button } from "primeng/button";
import { TooltipModule } from "primeng/tooltip";
import { DatePipe } from "@angular/common";
import { ScrollerModule } from "primeng/scroller";
import { Popover } from "primeng/popover";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { InputTextModule } from "primeng/inputtext";
import { ConfirmationService, MessageService } from "primeng/api";
import { ProgressBarModule } from "primeng/progressbar";
import { RadioButtonModule } from "primeng/radiobutton";
import { BlockUIModule } from "primeng/blockui";
import { PollService } from "./poll.service";
import { ConfirmPopupModule } from "primeng/confirmpopup";

export interface IPoll {
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  createdBy: string;
  options: { id: string; optionText: string; votes?: number }[];
  totalVotes: number;
  id: string;
  createdAt: string;
  updatedAt: string;

  creator: {
    id: string;
    username: string;
  };
}

export interface IPollDetails {
  pollDetails: IPoll;
  alreadyVoted: boolean;
  pendingVote: string | null;
}

@Component({
  selector: "app-poll",
  imports: [
    LogoComponent,
    Card,
    Button,
    TooltipModule,
    DatePipe,
    ScrollerModule,
    Popover,
    FormsModule,
    ReactiveFormsModule,
    InputTextModule,
    ProgressBarModule,
    RadioButtonModule,
    BlockUIModule,
    RouterModule,
    ConfirmPopupModule,
  ],
  templateUrl: "./poll.component.html",
  styleUrl: "./poll.component.scss",
})
export class PollComponent implements OnDestroy {
  private subscription: Subscription | null = null;
  private pollId: string | null = null;
  public poll: IPoll | null = null;
  public pollCreatedByLoggedUser = false;

  public pendingVote: string | "waiting-response" | null = null;
  public alreadyVoted = false;

  public newOptionText: string = "";
  public loadingNewOption: boolean = false;

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private pollService: PollService
  ) {
    this.subscription = this.route.queryParamMap.subscribe(async (params) => {
      await this.initializePoll(params.get("id"));
    });
  }

  private async initializePoll(pollId: string | null): Promise<void> {
    this.pollId = pollId;

    if (!this.pollId) {
      return;
    }

    try {
      const pollDetails = await this.pollService.loadPollDetails(this.pollId);

      if (pollDetails) {
        this.poll = pollDetails.pollDetails;
        this.alreadyVoted = pollDetails.alreadyVoted;
        this.pendingVote = pollDetails.pendingVote;

        this.pollCreatedByLoggedUser = await this.pollService.validateAccess(
          this.poll,
          this.pollId
        );
      } else throw new Error();
    } catch (error) {
      console.error("Erro ao carregar detalhes da enquete:", error);
      this.messageService.add({
        severity: "error",
        summary: "Erro",
        detail: "Não foi possível carregar os detalhes da enquete.",
      });

      try {
        await this.api.getUserProfile();
      } catch {
        this.messageService.add({
          severity: "error",
          summary: "Erro",
          detail: "Faça login para acessar a enquete.",
        });
      }

      this.router.navigate(["/"]);
    }
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
  }

  public async addOption() {
    if (!this.pollId || !this.newOptionText.trim()) {
      return;
    }

    this.loadingNewOption = true;

    try {
      const newOption = { optionText: this.newOptionText.trim() };
      const addedOption = await this.api.post<{
        id: string;
        optionText: string;
        votes?: number;
      }>(`polls/${this.pollId}/options`, newOption);
      this.poll?.options.push(addedOption);
      this.newOptionText = "";
      this.messageService.add({
        severity: "success",
        summary: "Opção adicionada",
        detail: "A nova opção foi adicionada com sucesso.",
      });

      await this.initializePoll(this.pollId);
    } catch (error) {
      console.error("Error adding option:", error);
      this.messageService.add({
        severity: "error",
        summary: "Erro ao adicionar opção",
        detail: "Não foi possível adicionar a nova opção.",
      });
    } finally {
      this.loadingNewOption = false;
    }
  }

  public cancel() {
    this.router.navigate(["/"]);
  }

  public async saveVote(remove: boolean = false) {
    if (
      !this.pollId ||
      (!this.pendingVote && !remove && this.pendingVote !== "waiting-response")
    ) {
      return;
    }

    try {
      await this.api.post(`polls/${this.pollId}/vote`, {
        optionId: remove ? null : this.pendingVote,
      });
      this.messageService.add({
        severity: "success",
        summary: "Voto registrado",
        detail: "Seu voto foi enviado.",
      });
      this.alreadyVoted = true;
      this.pendingVote = "waiting-response";
    } catch (error) {
      console.error("Error saving vote:", error);
      this.messageService.add({
        severity: "error",
        summary: "Erro ao registrar voto",
        detail: "Não foi possível registrar seu voto.",
      });
    }
  }

  public async removeOption(optionId: string, event: MouseEvent) {
    if (!this.pollId || !optionId) {
      return;
    }

    this.confirmationService.confirm({
      target: event.currentTarget as EventTarget,
      message: "Excluir opção?",
      icon: "pi pi-exclamation-triangle",
      rejectButtonProps: {
        severity: "secondary",
        outlined: true,
      },
      acceptLabel: "Confirmar Exclusão",
      rejectLabel: "Cancelar",
      accept: async () => {
        try {
          await this.api.delete(`polls/options/${optionId}`);
          (this.poll || { options: [] }).options = (
            this.poll?.options || []
          ).filter((opt) => opt.id !== optionId);
          this.messageService.add({
            severity: "success",
            summary: "Opção removida",
            detail: "A opção foi removida com sucesso.",
          });
        } catch (error) {
          console.error("Error removing option:", error);
          this.messageService.add({
            severity: "error",
            summary: "Erro ao remover opção",
            detail: "Não foi possível remover a opção.",
          });
        }
      },
      reject: () => {},
    });
  }

  public calcularPorcentagemVotos(qtd: number): number {
    if (!this.poll || this.poll.totalVotes === 0) return 0;
    return Number(((qtd / this.poll.totalVotes) * 100).toFixed(0));
  }
}
