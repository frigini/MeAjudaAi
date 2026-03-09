import styles from './auth.module.css';

/**
 * Renders the Auth UI container with a welcome heading.
 *
 * @returns A JSX element containing a container div styled via the component's CSS module and an `h1` with the text "Welcome to Auth!".
 */
export function Auth() {
  return (
    <div className={styles['container']}>
      <h1>Welcome to Auth!</h1>
    </div>
  );
}

export default Auth;
