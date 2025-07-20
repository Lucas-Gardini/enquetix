import { Component } from "@angular/core";
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
export class PollFormComponent {
  pollForm: FormGroup;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private messageService: MessageService,
    private router: Router
  ) {
    this.pollForm = this.fb.group({
      title: ["", Validators.required],
      description: [""],
      startDate: [null, Validators.required],
      endDate: [null, Validators.required],
    });
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

    this.loading = true;

    const payload = {
      ...this.pollForm.value,
      title: `${this.pollForm.value.title}`,
    };

    try {
      await this.api.post("polls", payload);

      this.messageService.add({
        severity: "success",
        summary: "Sucesso",
        detail: "Enquete criada com sucesso!",
      });

      this.pollForm.reset();
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
    this.router.navigate(["/"]);
  }
}
