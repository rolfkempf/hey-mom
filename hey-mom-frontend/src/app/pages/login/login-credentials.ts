export interface LoginCredentials {
  readonly email: string;
  readonly password: string;
}

export const developmentLogin: LoginCredentials | null = null;
