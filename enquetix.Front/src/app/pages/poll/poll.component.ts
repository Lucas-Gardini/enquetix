import { Component } from "@angular/core";
import { ActivatedRoute, Router } from "@angular/router";
import { ApiService } from "../../services/api.service";
import { Subscription } from "rxjs";
import { LogoComponent } from "../../components/logo/logo.component";
import { Card } from "primeng/card";
import { Button } from "primeng/button";

@Component({
  selector: "app-poll",
  imports: [LogoComponent, Card, Button],
  templateUrl: "./poll.component.html",
  styleUrl: "./poll.component.scss",
})
export class PollComponent {
  private subscription: Subscription | null = null;
  private pollId: string | null = null;

  constructor(
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.subscription = this.route.queryParams.subscribe((params) => {
      this.pollId = params?.["id"] ?? null;
      if (this.pollId) {
        this.loadPollDetails(this.pollId);
      }
    });
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
  }

  private async loadPollDetails(pollId: string) {
    try {
      const pollDetails = await this.api.get(`polls/${pollId}`);
      console.log("Poll Details:", pollDetails);
      // Handle the poll details as needed
    } catch (error) {
      console.error("Error loading poll details:", error);
    }
  }

  public cancel() {
    this.router.navigate(["/"]);
  }
}
