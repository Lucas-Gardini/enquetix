import { Component, OnDestroy, OnInit } from "@angular/core";
import { TableLazyLoadEvent, TableModule } from "primeng/table";
import { ApiService } from "../../services/api.service";
import { ActivatedRoute, RouterModule } from "@angular/router";
import { Subscription } from "rxjs";
import { DatePipe } from "@angular/common";

export interface Poll {
  id: string;
  title: string;
  description: string;
  startDate: string;
  endDate: string;
  createdBy: string;
  createdAt: string;
  updatedAt: string;
}

@Component({
  selector: "app-polls",
  imports: [TableModule, DatePipe, RouterModule],
  templateUrl: "./polls.component.html",
  styleUrl: "./polls.component.scss",
})
export class PollsComponent implements OnDestroy, OnInit {
  total: number = 0;
  polls: Poll[] = [];
  searchTerm: string = "";
  loading: boolean = false;
  firstLoaded: boolean = false;

  currentPage: number = 0;

  private subscription: Subscription | null = null;

  constructor(private api: ApiService, private route: ActivatedRoute) {
    this.subscription = this.route.queryParams.subscribe((params) => {
      this.searchTerm = params?.["search"] ?? "";
      this.searchPolls();
    });
  }

  ngOnInit() {
    this.searchPolls();
  }

  ngOnDestroy() {
    this.subscription?.unsubscribe();
  }

  public async searchPolls() {
    this.loading = true;

    try {
      const response = await this.api.get<{ polls: Poll[]; total: number }>(
        `polls?page=${this.currentPage}&search=${encodeURIComponent(
          this.searchTerm
        )}`
      );
      this.polls = response.polls;
      this.total = response.total;
    } catch (error) {
      console.error("Error searching polls:", error);
    } finally {
      if (!this.firstLoaded) this.firstLoaded = true;
      this.loading = false;
    }
  }

  public pageChange(event: TableLazyLoadEvent) {
    const page = Math.floor((event?.first ?? 0) / (event?.rows || 5));
    if (page !== this.currentPage) {
      this.currentPage = page;
      this.searchPolls();
    }
  }
}
