import { Component, Input, OnInit } from "@angular/core";
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { HttpClient } from "@angular/common/http";
import { MessageService } from "primeng/api";
import { Button, ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { PasswordModule } from "primeng/password";
import { InputTextModule } from "primeng/inputtext";
import { TextareaModule } from "primeng/textarea";
import { DatePickerModule } from "primeng/datepicker";
import { ApiService } from "../../services/api.service";
import { Router } from "@angular/router";
import { IPoll } from "../../pages/poll/poll.component";

@Component({
  selector: "app-poll-form",
  templateUrl: "./poll-form.component.html",
  styleUrls: ["./poll-form.component.scss"],
  providers: [MessageService],
  imports: [
    FormsModule,
    ReactiveFormsModule,
    CardModule,
    PasswordModule,
    ButtonModule,
    InputTextModule,
    TextareaModule,
    DatePickerModule,
  ],
})
export class PollFormComponent implements OnInit {
  pollForm: FormGroup;
  loading = false;

  @Input() pollData: IPoll | null = null;

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private messageService: MessageService,
    private router: Router
  ) {
    this.pollForm = this.fb.group({
      title: ["", Validators.required],
      description: [null, Validators.nullValidator],
      startDate: [null, Validators.nullValidator],
      endDate: [null, Validators.nullValidator],
    });
  }

  ngOnInit() {
    if (this.pollData) {
      this.pollForm.patchValue({
        title: this.pollData.title,
        description: this.pollData.description,
        startDate: new Date(this.pollData.startDate),
        endDate: new Date(this.pollData.endDate),
      });
    }
  }

  async onSubmit() {
    if (this.pollForm.invalid) {
      this.messageService.add({
        severity: "warn",
        summary: "Formulário inválido",
        detail: "Preencha todos os campos obrigatórios.",
      });
      return;
    }

    if (
      String(this.pollForm.value.title).trim() === "" ||
      this.pollForm.value.title.length < 5
    ) {
      this.messageService.add({
        severity: "warn",
        summary: "Título inválido",
        detail: "O título deve ter pelo menos 5 caracteres.",
      });
      return;
    }

    this.loading = true;

    const payload = {
      ...this.pollForm.value,
      title: `${this.pollForm.value.title}`,
    };

    try {
      if (!this.pollData) {
        await this.api.post("polls", payload);

        this.messageService.add({
          severity: "success",
          summary: "Sucesso",
          detail: "Enquete criada com sucesso!",
        });
      } else {
        if (!payload.startDate) payload.startDate = null;
        if (!payload.endDate) payload.endDate = null;

        await this.api.put(`polls/${this.pollData.id}`, payload);

        this.messageService.add({
          severity: "success",
          summary: "Sucesso",
          detail: "Enquete atualizada com sucesso!",
        });
      }

      this.cancel();
    } catch (err) {
      this.messageService.add({
        severity: "error",
        summary: "Erro",
        detail: "Falha ao criar a enquete.",
      });
    } finally {
      this.loading = false;
    }
  }

  cancel() {
    this.pollForm.reset();

    if (this.pollData) {
      this.router.navigate(["/poll"], {
        queryParams: { id: this.pollData.id },
      });
    } else this.router.navigate(["/"]);
  }
}
