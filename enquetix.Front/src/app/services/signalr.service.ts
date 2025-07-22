// src/app/services/signalr.service.ts
import { Injectable } from "@angular/core";
import {
  HubConnection,
  HubConnectionBuilder,
  LogLevel,
} from "@microsoft/signalr";
import { environment } from "../../environments/environment";

@Injectable({ providedIn: "root" })
export class PollHubService {
  private hubConnection!: HubConnection;

  public async startConnection(): Promise<void> {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/pollhub`)
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    await this.hubConnection.start();
  }

  public isConnected(): boolean {
    return this.hubConnection && this.hubConnection.state === "Connected";
  }

  public stopConnection(): void {
    this.hubConnection
      .stop()
      .then(() => console.log("[SignalR] Conexão encerrada"))
      .catch((err) =>
        console.error("[SignalR] Erro ao encerrar conexão:", err)
      );
  }

  public async subscribeToPoll(pollId: string): Promise<void> {
    if (!this.isConnected()) await this.startConnection();

    this.hubConnection
      .invoke("SubscribeToPoll", pollId)
      .catch((err) => console.error("Erro ao se inscrever:", err));
  }

  public unsubscribeFromPoll(pollId: string): void {
    this.hubConnection
      .invoke("UnsubscribeFromPoll", pollId)
      .catch((err) => console.error("Erro ao se desinscrever:", err));
  }

  public onPollUpdated(callback: (pollId: string, data: any) => void): void {
    this.hubConnection.on("PollUpdated", callback);
  }

  public onPollDeleted(callback: (pollId: string) => void): void {
    this.hubConnection.on("PollDeleted", callback);
  }

  public onPollOptionCreated(
    callback: (pollId: string, option: any) => void
  ): void {
    this.hubConnection.on("PollOptionCreated", callback);
  }

  public onPollOptionUpdated(
    callback: (pollId: string, option: any) => void
  ): void {
    this.hubConnection.on("PollOptionUpdated", callback);
  }

  public onPollOptionDeleted(
    callback: (pollId: string, option: any) => void
  ): void {
    this.hubConnection.on("PollOptionDeleted", callback);
  }

  public onPollVotesChanged(
    callback: (pollId: string, optionId: string, totalVotes: number) => void
  ): void {
    this.hubConnection.on("PollVotesChanged", callback);
  }
}
