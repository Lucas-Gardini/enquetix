import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";

@Injectable({ providedIn: "root" })
export class ApiService {
  constructor(private http: HttpClient) {}

  post<T>(endpoint: string, data: any): Promise<T> {
    return new Promise<T>((resolve, reject) => {
      this.http
        .post<T>(`${environment.apiUrl}/${endpoint}`, data, this.options())
        .subscribe({
          next: (response) => resolve(response),
          error: (error) => reject(error),
        });
    });
  }

  postFormData<T>(endpoint: string, data: Record<string, any>): Promise<T> {
    const formData = new FormData();

    Object.keys(data).forEach((key) => {
      if (data[key] !== null && data[key] !== undefined) {
        formData.append(key, data[key]);
      }
    });

    return new Promise<T>((resolve, reject) => {
      this.http
        .post<T>(`${environment.apiUrl}/${endpoint}`, formData, {
          ...this.options(),
        })
        .subscribe({
          next: (response) => resolve(response),
          error: (error) => reject(error),
        });
    });
  }

  get<T>(endpoint: string): Promise<T> {
    return new Promise<T>((resolve, reject) => {
      this.http
        .get<T>(`${environment.apiUrl}/${endpoint}`, this.options())
        .subscribe({
          next: (response) => resolve(response),
          error: (error) => reject(error),
        });
    });
  }

  put<T>(endpoint: string, data: any): Promise<T> {
    return new Promise<T>((resolve, reject) => {
      this.http
        .put<T>(`${environment.apiUrl}/${endpoint}`, data, this.options())
        .subscribe({
          next: (response) => resolve(response),
          error: (error) => reject(error),
        });
    });
  }

  delete<T>(endpoint: string): Promise<T> {
    return new Promise<T>((resolve, reject) => {
      this.http
        .delete<T>(`${environment.apiUrl}/${endpoint}`, this.options())
        .subscribe({
          next: (response) => resolve(response),
          error: (error) => reject(error),
        });
    });
  }

  patch<T>(endpoint: string, data: any): Promise<T> {
    return new Promise<T>((resolve, reject) => {
      this.http
        .patch<T>(`${environment.apiUrl}/${endpoint}`, data, this.options())
        .subscribe({
          next: (response) => resolve(response),
          error: (error) => reject(error),
        });
    });
  }

  options() {
    return {
      withCredentials: true,
    };
  }

  //#region Métodos Específicos
  getUserProfile(): Promise<{ id: string; username: string }> {
    return this.get("auth/me");
  }

  logout(): Promise<void> {
    return new Promise<void>((resolve, reject) => {
      this.http
        .post(`${environment.apiUrl}/auth/logout`, {}, this.options())
        .subscribe({
          next: () => resolve(window.location.reload()),
          error: (error) => reject(error),
        });
    });
  }
  //#endregion
}
