import { Component } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { ConfirmationService, MessageService } from "primeng/api";
import { ToastModule } from "primeng/toast";
import { ConfirmPopupModule } from "primeng/confirmpopup";

@Component({
  selector: "app-root",
  imports: [RouterOutlet, ToastModule, ConfirmPopupModule],
  providers: [MessageService, ConfirmationService],
  templateUrl: "./app.component.html",
  styleUrl: "./app.component.scss",
})
export class AppComponent {
  title = "Enquetix";
}
