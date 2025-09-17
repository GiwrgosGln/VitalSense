export type BaseEntity = {
  id: string;
  createdAt: number;
  updatedAt: number;
};

export type Entity<T> = {
  [K in keyof T]: T[K];
} & BaseEntity;

export type Client = Entity<{
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  dateOfBirth?: string;
  gender?: string;
  hasCard: boolean;
  notes?: string;
}>;

export type User = Entity<{
  username: string;
  email: string;
}>;

export type AuthResponse = {
  accessToken: string;
  accessTokenExpiry: string;
  refreshToken: string;
  refreshTokenExpiry: string;
  user: User;
};
