declare const DEV_TEST_EMAIL: string | undefined;
declare const DEV_TEST_PASSWORD: string | undefined;

export const developmentLogin =
  typeof DEV_TEST_EMAIL === 'string' && DEV_TEST_EMAIL &&
  typeof DEV_TEST_PASSWORD === 'string' && DEV_TEST_PASSWORD
    ? { email: DEV_TEST_EMAIL, password: DEV_TEST_PASSWORD }
    : null;
