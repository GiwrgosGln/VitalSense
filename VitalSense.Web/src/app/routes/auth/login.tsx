import { paths } from "@/config/paths";
import { LoginForm } from "@/features/auth/components/login-form";
import { useNavigate, useSearchParams } from "react-router-dom";

const LoginRoute = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const redirectTo = searchParams.get("redirectTo");

  return (
    <LoginForm
      onSuccess={() => {
        navigate(`${redirectTo ? `${redirectTo}` : paths.home.getHref()}`, {
          replace: true,
        });
      }}
    />
  );
};

export default LoginRoute;
