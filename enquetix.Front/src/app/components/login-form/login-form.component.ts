import { Component, OnInit } from "@angular/core";
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { MessageService } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { PasswordModule } from "primeng/password";
import { InputTextModule } from "primeng/inputtext";
import { ApiService } from "../../services/api.service";

@Component({
  selector: "app-login-form",
  imports: [
    FormsModule,
    ReactiveFormsModule,
    CardModule,
    PasswordModule,
    ButtonModule,
    InputTextModule,
  ],
  templateUrl: "./login-form.component.html",
  styleUrl: "./login-form.component.scss",
})
export class LoginFormComponent implements OnInit {
  authForm: FormGroup;
  isLoginMode = true;
  isLoggedIn = false;
  loggedUser: { id: string; username: string } | null = null;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private messageService: MessageService,
    public api: ApiService
  ) {
    this.authForm = this.fb.group({
      email: ["", [Validators.required, Validators.email]],
      password: ["", [Validators.required, Validators.minLength(6)]],
    });
  }

  async ngOnInit() {
    const logged = await this.api.getUserProfile();
    if (logged) {
      this.isLoggedIn = true;
      this.loggedUser = {
        id: logged.id,
        username: logged.username,
      };
    }
  }

  toggleMode() {
    this.isLoginMode = !this.isLoginMode;

    if (this.isLoginMode) {
      this.authForm.removeControl("username");
      this.authForm.removeControl("confirmPassword");
    } else {
      this.authForm.addControl(
        "username",
        this.fb.control("", [Validators.required, Validators.minLength(3)])
      );
      this.authForm.addControl(
        "confirmPassword",
        this.fb.control("", [Validators.required])
      );
    }

    this.authForm.reset();
  }

  async onSubmit() {
    if (this.authForm.valid) {
      this.loading = true;

      try {
        await this.api.postFormData("auth/login", this.authForm.value);
        this.messageService.add({
          severity: "success",
          summary: "Sucesso",
          detail: "Login realizado com sucesso!",
        });

        this.isLoggedIn = true;
      } catch (error) {
        this.messageService.add({
          severity: "error",
          summary: "Erro",
          detail: "Falha ao realizar login. Verifique suas credenciais.",
        });
      }

      this.loading = false;
    } else {
      this.messageService.add({
        severity: "error",
        summary: "Error",
        detail: "Formulário inválido",
      });
    }
  }

  getFieldError(fieldName: string): string {
    const field = this.authForm.get(fieldName);
    if (field?.errors && field.touched) {
      if (field.errors["required"]) {
        return `Campo obrigatório`;
      }
      if (field.errors["email"]) {
        return "Insira um endereço de e-mail válido";
      }
      if (field.errors["minlength"]) {
        return "A senha deve ter pelo menos 6 caracteres";
      }
    }
    return "";
  }

  passwordsMatch(): boolean {
    if (this.isLoginMode) return true;

    const password = this.authForm.get("password")?.value;
    const confirmPassword = this.authForm.get("confirmPassword")?.value;
    return password === confirmPassword;
  }
}
