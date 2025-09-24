import { AuthLayout } from "@/components/layouts/auth-layout";
import { paths } from "@/config/paths";
import { RegisterForm } from "@/features/auth/components/register-form";
import { useNavigate, useSearchParams } from "react-router-dom";

const RegisterRoute = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const redirectTo = searchParams.get("redirectTo");
  return (
    <AuthLayout>
      <RegisterForm
        onSuccess={() => {
          navigate(`${redirectTo ? `${redirectTo}` : paths.home.getHref()}`),
            {
              replace: true,
            };
        }}
      />
    </AuthLayout>
  );
};

export default RegisterRoute;
