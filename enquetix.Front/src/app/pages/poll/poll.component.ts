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
import { PollHubService } from "../../services/signalr.service";

export interface IPoll {
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  createdBy: string;
  options: { id: string; optionText: string; totalVotes?: number }[];
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

  public pendingVote: string | null = null;
  public alreadyVoted = false;

  public newOptionText: string = "";
  public loadingNewOption: boolean = false;

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private pollService: PollService,
    private pollHub: PollHubService
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

        await this.pollHub.subscribeToPoll(this.pollId);
        this.subscribeToEvents();
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

  subscribeToEvents(): void {
    this.pollHub.onPollUpdated((pollId, data) => {
      this.poll = data;
    });

    this.pollHub.onPollDeleted((pollId) => {
      this.messageService.add({
        severity: "info",
        summary: "Enquete deletada",
        detail: "A enquete foi deletada.",
      });
      this.router.navigate(["/"]);
    });

    this.pollHub.onPollOptionCreated(
      (pollId, option: IPoll["options"][number]) => {
        if (!this.poll) return;
        if (!this.poll.options) this.poll.options = [];

        if (this.poll.options.some((opt) => opt.id === option.id)) {
          console.warn(
            `Opção com ID ${option.id} já existe na enquete ${pollId}`
          );
          return;
        }

        this.poll?.options.push(option);
      }
    );

    this.pollHub.onPollOptionUpdated((pollId, option) => {
      if (!this.poll) return;

      const existingOption = this.poll.options.find(
        (opt) => opt.id === option.id
      );
      if (existingOption) {
        existingOption.optionText = option.optionText;
        existingOption.totalVotes = option.totalVotes;

        this.poll.totalVotes = this.poll.options.reduce(
          (total, opt) => total + (opt.totalVotes || 0),
          0
        );
      } else {
        console.warn(
          `Opção com ID ${option.id} não encontrada na enquete ${pollId}`
        );
      }
    });

    this.pollHub.onPollOptionDeleted((pollId, option) => {
      if (!this.poll) return;

      this.poll.options = this.poll.options.filter(
        (opt) => opt.id !== option.id
      );
    });

    this.pollHub.onPollVotesChanged((pollId, optionId, totalVotes) => {
      if (!this.poll) return;

      const option = this.poll.options.find((opt) => opt.id === optionId);
      if (option) {
        option.totalVotes = totalVotes;

        this.poll.totalVotes = this.poll.options.reduce(
          (total, opt) => total + (opt.totalVotes || 0),
          0
        );
      } else {
        console.warn(
          `Opção com ID ${optionId} não encontrada na enquete ${pollId}`
        );
      }
    });
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
    this.pollHub.unsubscribeFromPoll(this.pollId!);
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
    if (!this.pollId || (!this.pendingVote && !remove)) {
      return;
    }

    try {
      await this.api.post(`polls/${this.pollId}/vote`, {
        optionId: remove ? null : this.pendingVote,
      });

      if (!remove) {
        this.messageService.add({
          severity: "success",
          summary: "Voto registrado",
          detail:
            "Seu voto foi enviado. Pode levar alguns instantes para ser processado.",
        });
        this.alreadyVoted = true;
      } else {
        this.pendingVote = null;
      }
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

  public async deletePoll(event: MouseEvent) {
    if (!this.pollId) {
      return;
    }

    this.confirmationService.confirm({
      target: event.currentTarget as EventTarget,
      message: "Excluir enquete?",
      icon: "pi pi-exclamation-triangle",
      rejectButtonProps: {
        severity: "secondary",
        outlined: true,
      },
      acceptLabel: "Confirmar Exclusão",
      rejectLabel: "Cancelar",
      accept: async () => {
        try {
          await this.api.delete(`polls/${this.pollId}`);
          this.messageService.add({
            severity: "success",
            summary: "Enquete excluída",
            detail: "A enquete foi excluída com sucesso.",
          });
          this.router.navigate(["/"]);
        } catch (error) {
          console.error("Error deleting poll:", error);
          this.messageService.add({
            severity: "error",
            summary: "Erro ao excluir enquete",
            detail: "Não foi possível excluir a enquete.",
          });
        }
      },
      reject: () => {},
    });
  }

  public copyPollLink() {
    if (!this.pollId) return;

    const pollUrl = window.location.href;
    navigator.clipboard.writeText(pollUrl).then(
      () => {
        this.messageService.add({
          severity: "success",
          summary: "Link copiado",
          detail: "O link da enquete foi copiado para a área de transferência.",
        });
      },
      (err) => {
        console.error("Erro ao copiar o link:", err);
        this.messageService.add({
          severity: "error",
          summary: "Erro ao copiar",
          detail: "Não foi possível copiar o link da enquete.",
        });
      }
    );
  }

  public calcularPorcentagemVotos(qtd: number): number {
    if (!this.poll || this.poll.totalVotes === 0) return 0;
    return Number(((qtd / this.poll.totalVotes) * 100).toFixed(0));
  }
}
