import { Injectable } from "@angular/core";
import { ApiService } from "../../services/api.service";
import { Router } from "@angular/router";
import { IPoll, IPollDetails } from "./poll.component";

@Injectable({
  providedIn: "root",
})
export class PollService {
  constructor(private api: ApiService, private router: Router) {}

  public async validateAccess(poll: IPoll, pollId: string): Promise<boolean> {
    if (!pollId) {
      this.router.navigate(["/"]);
      return false;
    }

    let loggedUser: { id: string } | null = null;

    try {
      loggedUser = await this.api.getUserProfile();
      if (!loggedUser) {
        this.router.navigate(["/"]);
        return false;
      }
    } catch (error) {
      this.router.navigate(["/"]);
      return false;
    }

    return poll?.createdBy === loggedUser?.id;
  }

  public async loadPollDetails(pollId: string): Promise<IPollDetails | null> {
    try {
      const pollDetails = await this.api.get<IPoll>(`polls/${pollId}`);

      const myVote = await this.api
        .get<{
          optionId: string;
          optionText: string;
        }>(`polls/${pollId}/vote`)
        .catch(() => {
          return { optionId: null, optionText: "" };
        });

      return {
        pollDetails,
        alreadyVoted: !!myVote.optionId,
        pendingVote: myVote.optionId ?? null,
      };
    } catch (error) {
      console.error("Error loading poll details:", error);
    }

    return null;
  }
}
